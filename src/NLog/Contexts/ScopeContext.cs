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
        private static IEqualityComparer<string> DefaultComparer = StringComparer.OrdinalIgnoreCase;

#if !NET35 && !NET40
        /// <summary>
        /// Pushes new state on the logical context scope stack together with provided properties
        /// </summary>
        /// <param name="nestedState">Value to added to the scope stack</param>
        /// <param name="properties">Properties being added to the scope dictionary</param>
        /// <returns>A disposable object that pops the nested scope state on dispose (including properties).</returns>
        /// <remarks>Scope dictionary keys are case-insensitive</remarks>
        public static IDisposable PushNestedStateProperties(object nestedState, IReadOnlyCollection<KeyValuePair<string, object>> properties)
        {
            if (properties?.Count > 0)
            {
#if !NET45
                var parent = GetAsyncLocalContext();
                var current = new ScopeContextProperties<object>(parent, properties, nestedState);
                SetAsyncLocalContext(current);
                return current;
#else
                var oldMappedContext = PushPropertiesCallContext(properties);
                var oldNestedContext = PushNestedStateCallContext(nestedState);
                return new ScopeContextNestedStateProperties(oldNestedContext, oldMappedContext);
#endif
            }
            else
            {
                return PushNestedState(nestedState);
            }
        }
#endif

#if !NET35 && !NET40
        /// <summary>
        /// Updates the logical scope context with provided properties
        /// </summary>
        /// <param name="properties">Properties being added to the scope dictionary</param>
        /// <returns>A disposable object that removes the properties from logical context scope on dispose.</returns>
        /// <remarks>Scope dictionary keys are case-insensitive</remarks>
        public static IDisposable PushProperties(IReadOnlyCollection<KeyValuePair<string, object>> properties)
        {
            return PushProperties<object>(properties);
        }

        /// <summary>
        /// Updates the logical scope context with provided properties
        /// </summary>
        /// <param name="properties">Properties being added to the scope dictionary</param>
        /// <returns>A disposable object that removes the properties from logical context scope on dispose.</returns>
        /// <remarks>Scope dictionary keys are case-insensitive</remarks>
        public static IDisposable PushProperties<TValue>(IReadOnlyCollection<KeyValuePair<string, TValue>> properties)
        {
#if !NET45
            var parent = GetAsyncLocalContext();
            
            var allProperties = ScopeContextPropertiesCollapsed.BuildCollapsedDictionary(parent, properties.Count);
            if (allProperties != null)
            {
                // Collapse all 3 property-scopes into a collapsed scope, and return bookmark that can restore original parent (Avoid huge object-graphs)
                CopyScopePropertiesToDictionary(properties, allProperties);

                var collapsedState = new ScopeContextProperties<object>(parent.Parent.Parent, allProperties);
                SetAsyncLocalContext(collapsedState);
                return new ScopeContextPropertiesCollapsed(parent, collapsedState);
            }

            var current = new ScopeContextProperties<TValue>(parent, properties, null);
            SetAsyncLocalContext(current);
            return current;
#else
            var oldContext = PushPropertiesCallContext(properties);
            return new ScopeContextProperties(oldContext);
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
        public static IDisposable PushProperty<TValue>(string key, TValue value)
        {
#if !NET35 && !NET40 && !NET45
            var parent = GetAsyncLocalContext();

            var scopeProperties = ScopeContextPropertiesCollapsed.BuildCollapsedDictionary(parent, 1);
            if (scopeProperties != null)
            {
                // Collapse all 3 property-scopes into a collapsed scope, and return bookmark that can restore original parent (Avoid huge object-graphs)
                scopeProperties[key] = value;

                var collapsedState = new ScopeContextProperties<object>(parent.Parent.Parent, scopeProperties);
                SetAsyncLocalContext(collapsedState);
                return new ScopeContextPropertiesCollapsed(parent, collapsedState);
            }

            var current = new ScopeContextProperty<TValue>(parent, key, value);
            SetAsyncLocalContext(current);
            return current;
#else
            var oldContext = PushPropertyCallContext(key, value);
            return new ScopeContextProperties(oldContext);
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
            var parent = GetAsyncLocalContext();
            var current = new ScopeContextNestedState<T>(parent, nestedState);
            SetAsyncLocalContext(current);
            return current;
#else
            object objectValue = nestedState;
            var oldNestedContext = PushNestedStateCallContext(objectValue);
            return new ScopeContextNestedState(oldNestedContext, objectValue);
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
            SetAsyncLocalContext(null);
#else
            ClearMappedContextCallContext();
            ClearNestedContextCallContext();
#endif
        }

        /// <summary>
        /// Retrieves all properties stored within the logical context scopes
        /// </summary>
        /// <returns>Collection of all properties</returns>
        public static IEnumerable<KeyValuePair<string, object>> GetAllProperties()
        {
#if !NET35 && !NET40 && !NET45
            var contextState = GetAsyncLocalContext();
            return contextState?.CaptureContextProperties(0, out var _) ?? ArrayHelper.Empty<KeyValuePair<string, object>>();
#else
            var mappedContext = GetMappedContextCallContext();
            if (mappedContext?.Count > 0)
            {
                foreach (var item in mappedContext)
                {
                    if (item.Value is ObjectHandleSerializer)
                    {
                        return GetAllPropertiesUnwrapped(mappedContext);
                    }
                }
                return mappedContext;
            }
            return ArrayHelper.Empty<KeyValuePair<string, object>>();
#endif
        }

        internal static ScopePropertiesEnumerator<object> GetAllPropertiesEnumerator()
        {
            return new ScopePropertiesEnumerator<object>(GetAllProperties());
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
            var contextState = GetAsyncLocalContext();
            if (contextState != null)
            {
                var mappedContext = contextState?.CaptureContextProperties(0, out var _);
                if (mappedContext != null)
                {
                    return TryLookupProperty(mappedContext, key, out value);
                }
            }
            value = null;
            return false;
#else
            var mappedContext = GetMappedContextCallContext();
            if (mappedContext != null && mappedContext.TryGetValue(key, out value))
            {
                if (value is ObjectHandleSerializer objectHandle)
                    value = objectHandle.Unwrap();
                return true;
            }

            value = null;
            return false;
#endif
        }

        /// <summary>
        /// Retrieves all nested states inside the logical context scope stack
        /// </summary>
        /// <returns>Array of nested state objects.</returns>
        public static object[] GetAllNestedStates()
        {
#if !NET35 && !NET40 && !NET45
            var parent = GetAsyncLocalContext();
            return parent?.CaptureNestedContext(0, out var _) ?? ArrayHelper.Empty<object>();
#else
            var currentContext = GetNestedContextCallContext();
            if (currentContext?.Count > 0)
            {
                int index = 0;
                object[] messages = new object[currentContext.Count];
                foreach (var node in currentContext)
                {
                    if (node is ObjectHandleSerializer objectHandle)
                        messages[index++] = objectHandle.Unwrap();
                    else
                        messages[index++] = node;
                }
                return messages;
            }
            return ArrayHelper.Empty<object>();
#endif
        }

        /// <summary>
        /// Peeks the top value from the logical context scope stack
        /// </summary>
        /// <returns>Value from the top of the stack.</returns>
        public static object PeekNestedState()
        {
#if !NET35 && !NET40 && !NET45
            var parent = GetAsyncLocalContext();
            while (parent != null)
            {
                var nestedContext = parent.NestedState;
                if (nestedContext != null)
                    return nestedContext;

                parent = parent.Parent;
            }
            return null;
#else
            var currentContext = GetNestedContextCallContext();
            var objectValue = currentContext?.Count > 0 ? currentContext.First.Value : null;
            if (objectValue is ObjectHandleSerializer objectHandle)
                objectValue = objectHandle.Unwrap();
            return objectValue;
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
            var parent = GetAsyncLocalContext();
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
            return default(TimeSpan?);  // Delay timing only supported when using AsyncLocal. CallContext is sensitive to custom state
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
            var parent = GetAsyncLocalContext();
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
            return default(TimeSpan?);  // Delay timing only supported when using AsyncLocal. CallContext is sensitive to custom state
#endif
        }

#if !NET35 && !NET40 && !NET45
        private static bool TryLookupProperty(IReadOnlyCollection<KeyValuePair<string, object>> scopeProperties, string key, out object value)
        {
            if (scopeProperties is Dictionary<string, object> mappedDictionary && ReferenceEquals(mappedDictionary.Comparer, DefaultComparer))
            {
                return mappedDictionary.TryGetValue(key, out value);
            }
            else
            {
                using (var scopeEnumerator = new ScopePropertiesEnumerator<object>(scopeProperties))
                {
                    while (scopeEnumerator.MoveNext())
                    {
                        var item = scopeEnumerator.Current;
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

        private static Dictionary<string, object> CloneParentContextDictionary(IReadOnlyCollection<KeyValuePair<string, object>> parentContext, int initialCapacity)
        {
            var propertyCount = parentContext.Count;
            var scopeProperties = new Dictionary<string, object>(propertyCount + initialCapacity, DefaultComparer);
            if (propertyCount > 0)
            {
                CopyScopePropertiesToDictionary(parentContext, scopeProperties);
            }
            return scopeProperties;
        }

        private static void CopyScopePropertiesToDictionary<TValue>(IReadOnlyCollection<KeyValuePair<string, TValue>> scopeProperties, Dictionary<string, object> scopeDictionary)
        {
            using (var propertyEnumerator = new ScopePropertiesEnumerator<TValue>(scopeProperties))
            {
                while (propertyEnumerator.MoveNext())
                {
                    var item = propertyEnumerator.Current;
                    scopeDictionary[item.Key] = item.Value;
                }
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
                SetAsyncLocalContext(_parent);
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
                    SetAsyncLocalContext(Parent);
                    _disposed = true;
                }
            }
        }

        private sealed class ScopeContextProperty<TValue> : IPropertyScopeContext
        {
            public IScopeContext Parent { get; }
            long IScopeContext.NestedStateTimestamp => 0;
            object IScopeContext.NestedState => null;
            public string Name { get; }
            public TValue Value { get; }
            private IReadOnlyCollection<KeyValuePair<string, object>> _allProperties;
            private bool _disposed;

            public ScopeContextProperty(IScopeContext parent, string name, TValue value)
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
                if (_allProperties != null)
                {
                    return _allProperties;
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
                                return _allProperties = new[] { new KeyValuePair<string, object>(Name, Value) };
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
                        _allProperties = scopeProperties; // Immutable since no more scope-properties
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
                    SetAsyncLocalContext(Parent);
                    _disposed = true;
                }
            }
        }

        private sealed class ScopeContextProperties<TValue> : IPropertyScopeContext, IReadOnlyCollection<KeyValuePair<string, object>>
        {
            public IScopeContext Parent { get; }
            public long NestedStateTimestamp { get; }
            public object NestedState { get; }

            private readonly IReadOnlyCollection<KeyValuePair<string, TValue>> _scopeProperties;
            private IReadOnlyCollection<KeyValuePair<string, object>> _allProperties;
            private bool _disposed;

            public ScopeContextProperties(IScopeContext parent, Dictionary<string, object> allProperties)
            {
                Parent = parent;
                _allProperties = allProperties; // Collapsed dictionary that includes all properties from parent scopes with case-insensitive-comparer
            }

            public ScopeContextProperties(IScopeContext parent, IReadOnlyCollection<KeyValuePair<string, TValue>> scopeProperties, object nestedState)
            {
                Parent = parent;
                _scopeProperties = scopeProperties;
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
                if (_allProperties != null)
                {
                    return _allProperties;
                }
                else
                {
                    var parentContext = Parent?.CaptureContextProperties(initialCapacity + _scopeProperties.Count, out scopeProperties);
                    if (scopeProperties == null)
                    {
                        if (parentContext == null)
                        {
                            // No more parent-context, build scope-property-collection starting from this scope
                            if (initialCapacity == 0)
                                return _allProperties = EnsureCollectionWithUniqueKeys();
                            else
                                scopeProperties = new Dictionary<string, object>(initialCapacity + _scopeProperties.Count, DefaultComparer);
                        }
                        else
                        {
                            // Build scope-property-collection from parent-context
                            scopeProperties = CloneParentContextDictionary(parentContext, initialCapacity + _scopeProperties.Count);
                        }
                    }

                    AppendScopeProperties(scopeProperties);
                    if (initialCapacity == 0)
                        _allProperties = scopeProperties; // Immutable since no more scope-properties
                    return scopeProperties;
                }
            }

            private IReadOnlyCollection<KeyValuePair<string, object>> EnsureCollectionWithUniqueKeys()
            {
                var propertyCount = _scopeProperties.Count;
                if (propertyCount > 10)
                {
                    var scopeDictionary = new Dictionary<string, object>(_scopeProperties.Count, DefaultComparer);
                    AppendScopeProperties(scopeDictionary);
                    return scopeDictionary;
                }

                if (propertyCount > 1 && !ScopePropertiesEnumerator<TValue>.HasUniqueCollectionKeys(_scopeProperties, DefaultComparer))
                {
                    var scopeDictionary = new Dictionary<string, object>(_scopeProperties.Count, DefaultComparer);
                    AppendScopeProperties(scopeDictionary);
                    return scopeDictionary;
                }

                return _scopeProperties as IReadOnlyCollection<KeyValuePair<string, object>> ?? this;
            }

            private void AppendScopeProperties(Dictionary<string, object> scopeDictionary)
            {
                CopyScopePropertiesToDictionary(_scopeProperties, scopeDictionary);
            }

            public override string ToString()
            {
                return NestedState?.ToString() ?? $"Count = {Count}";
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    SetAsyncLocalContext(Parent);
                    _disposed = true;
                }
            }

            public int Count => _scopeProperties.Count;

            IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
            {
                foreach (var property in _scopeProperties)
                    yield return new KeyValuePair<string, object>(property.Key, property.Value);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                foreach (var property in _scopeProperties)
                    yield return new KeyValuePair<string, object>(property.Key, property.Value);
            }
        }

        [Obsolete("Replaced by ScopeContext.PushProperty / ScopeContext.PushNestedState")]
        private sealed class LegacyScopeContext : IScopeContext
        {
            public IScopeContext Parent => null;    // Always top parent
            public object[] NestedContext { get; }
            public IReadOnlyCollection<KeyValuePair<string, object>> MappedContext { get; }
            public long NestedStateTimestamp { get; }
            private bool _disposed;

            public LegacyScopeContext(IReadOnlyCollection<KeyValuePair<string, object>> allProperties, object[] nestedContext, long nestedContextTimestamp)
            {
                MappedContext = allProperties;
                NestedContext = nestedContext;
                NestedStateTimestamp = nestedContextTimestamp;
            }

            public static void CaptureLegacyContext(IScopeContext contextState, out Dictionary<string, object> allProperties, out object[] nestedContext, out long nestedContextTimestamp)
            {
                nestedContext = contextState?.CaptureNestedContext(0, out var _) ?? ArrayHelper.Empty<object>();
                allProperties = null;
                var scopeProperties = contextState?.CaptureContextProperties(0, out allProperties) ?? ArrayHelper.Empty<KeyValuePair<string, object>>();
                if (allProperties == null)
                {
                    allProperties = new Dictionary<string, object>(scopeProperties.Count, DefaultComparer);
                    CopyScopePropertiesToDictionary(scopeProperties, allProperties);
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
                    SetAsyncLocalContext(Parent);
                    _disposed = true;
                }
            }
        }

        private static void SetAsyncLocalContext(IScopeContext newValue)
        {
            AsyncNestedDiagnosticsContext.Value = newValue;
        }

        private static IScopeContext GetAsyncLocalContext()
        {
            return AsyncNestedDiagnosticsContext.Value;
        }


        private static readonly System.Threading.AsyncLocal<IScopeContext> AsyncNestedDiagnosticsContext = new System.Threading.AsyncLocal<IScopeContext>();
#endif

#if NET45
        private sealed class ScopeContextNestedStateProperties : IDisposable
        {
            private readonly LinkedList<object> _parentNestedContext;
            private readonly Dictionary<string, object> _parentMappedContext;

            public ScopeContextNestedStateProperties(LinkedList<object> parentNestedContext, Dictionary<string, object> parentMappedContext)
            {
                _parentNestedContext = parentNestedContext;
                _parentMappedContext = parentMappedContext;
            }

            public void Dispose()
            {
                SetNestedContextCallContext(_parentNestedContext);
                SetMappedContextCallContext(_parentMappedContext);
            }
        }
#endif

        internal struct ScopePropertiesEnumerator<TValue> : IEnumerator<KeyValuePair<string, object>>
        {
            readonly IEnumerator<KeyValuePair<string, object>> _scopeEnumerator;
#if !NET35 && !NET40
            readonly IReadOnlyList<KeyValuePair<string, object>> _scopeList;
            int _scopeIndex;
#endif
            Dictionary<string, object>.Enumerator _dicationaryEnumerator;

            public ScopePropertiesEnumerator(IEnumerable<KeyValuePair<string, TValue>> scopeProperties)
            {
#if !NET35 && !NET40
                if (scopeProperties is IReadOnlyList<KeyValuePair<string, object>> scopeList)
                {
                    _scopeEnumerator = null;
                    _scopeList = scopeList;
                    _scopeIndex = -1;
                    _dicationaryEnumerator = default(Dictionary<string, object>.Enumerator);
                    return;
                }
                else
                {
                    _scopeList = null;
                    _scopeIndex = 0;
                }
#endif

                if (scopeProperties is Dictionary<string, object> scopeDictionary)
                {
                    _scopeEnumerator = null;
                    _dicationaryEnumerator = scopeDictionary.GetEnumerator();
                }
                else if (scopeProperties is IEnumerable<KeyValuePair<string, object>> scopeEnumerator)
                {
                    _scopeEnumerator = scopeEnumerator.GetEnumerator();
                    _dicationaryEnumerator = default(Dictionary<string, object>.Enumerator);
                }
                else
                {
                    _scopeEnumerator = CreateScopeEnumerable(scopeProperties).GetEnumerator();
                    _dicationaryEnumerator = default(Dictionary<string, object>.Enumerator);
                }
            }

            public static bool HasUniqueCollectionKeys(IEnumerable<KeyValuePair<string, TValue>> scopeProperties, IEqualityComparer<string> keyComparer)
            {
                int startIndex = 1;
                using (var leftEnumerator = new ScopePropertiesEnumerator<TValue>(scopeProperties))
                {
                    while (leftEnumerator.MoveNext())
                    {
                        ++startIndex;

                        int currentIndex = 0;

                        var left = leftEnumerator.Current;
                        using (var rightEnumerator = new ScopePropertiesEnumerator<TValue>(scopeProperties))
                        {
                            while (rightEnumerator.MoveNext())
                            {
                                if (++currentIndex < startIndex)
                                    continue;

                                var right = rightEnumerator.Current;
                                if (keyComparer.Equals(left.Key, right.Key))
                                {
                                    return false;
                                }

                                if (currentIndex > 10)
                                {
                                    return false;   // Too many comparisons
                                }
                            }
                        }
                    }
                }

                return true;
            }

            private static IEnumerable<KeyValuePair<string, object>> CreateScopeEnumerable(IEnumerable<KeyValuePair<string, TValue>> scopeProperties)
            {
                foreach (var property in scopeProperties)
                    yield return new KeyValuePair<string, object>(property.Key, property.Value);
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

        [Obsolete("Replaced by ScopeContext.PushProperty. Marked obsolete on NLog 5.0")]
        internal static void SetMappedContextLegacy<TValue>(string key, TValue value)
        {
#if !NET35 && !NET40 && !NET45
            PushProperty(key, value);
#else
            PushPropertyCallContext(key, value);
#endif
        }

        internal static ICollection<string> GetKeysMappedContextLegacy()
        {
#if !NET35 && !NET40 && !NET45
            using (var propertyEnumerator = GetAllPropertiesEnumerator())
            {
                if (!propertyEnumerator.MoveNext())
                    return ArrayHelper.Empty<string>();

                var firstProperty = propertyEnumerator.Current;
                if (!propertyEnumerator.MoveNext())
                    return new[] { firstProperty.Key };

                var propertyKeys = new List<string>();
                propertyKeys.Add(firstProperty.Key);
                do
                {
                    propertyKeys.Add(propertyEnumerator.Current.Key);
                } while (propertyEnumerator.MoveNext());
                return propertyKeys;
            }
#else
            return GetMappedContextCallContext()?.Keys ?? (ICollection<string>)ArrayHelper.Empty<string>();
#endif
        }

        [Obsolete("Replaced by disposing return value from ScopeContext.PushProperty. Marked obsolete on NLog 5.0")]
        internal static void RemoveMappedContextLegacy(string key)
        {
#if !NET35 && !NET40 && !NET45
            if (TryGetProperty(key, out var _))
            {
                // Replace with new legacy-scope, the legacy-scope can be discarded when previous parent scope is restored
                var contextState = GetAsyncLocalContext();
                LegacyScopeContext.CaptureLegacyContext(contextState, out var allProperties, out var scopeNestedStates, out var scopeNestedStateTimestamp);
                allProperties.Remove(key);

                var legacyScope = new LegacyScopeContext(allProperties, scopeNestedStates, scopeNestedStateTimestamp);
                SetAsyncLocalContext(legacyScope);
            }
#else
            var oldContext = GetMappedContextCallContext();
            if (oldContext?.ContainsKey(key) == true)
            {
                var newContext = CloneMappedContext(oldContext, 0);
                newContext.Remove(key);
                SetMappedContextCallContext(newContext);
            }
#endif
        }

        [Obsolete("Replaced by disposing return value from ScopeContext.PushNestedState. Marked obsolete on NLog 5.0")]
        internal static object PopNestedContextLegacy()
        {
#if !NET35 && !NET40 && !NET45
            var contextState = GetAsyncLocalContext();
            if (contextState != null)
            {
                if ((contextState.Parent == null && contextState is LegacyScopeContext) || contextState.NestedState == null)
                {
                    var nestedContext = contextState?.CaptureNestedContext(0, out var _) ?? ArrayHelper.Empty<object>();
                    if (nestedContext.Length == 0)
                        return null;    // Nothing to pop, just leave scope alone

                    // Replace with new legacy-scope, the legacy-scope can be discarded when previous parent scope is restored
                    var stackTopValue = nestedContext[0];
                    var allProperties = contextState?.CaptureContextProperties(0, out var _) ?? ArrayHelper.Empty<KeyValuePair<string, object>>();
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

                    var legacyScope = new LegacyScopeContext(allProperties, nestedContext, nestedContext.Length > 0 ? GetNestedContextTimestampNow() : 0L);
                    SetAsyncLocalContext(legacyScope);
                    return stackTopValue;
                }
                else
                {
                    SetAsyncLocalContext(contextState.Parent);
                    return contextState?.NestedState;
                }
            }
            return null;
#else
            var currentContext = GetNestedContextCallContext();
            if (currentContext?.Count > 0)
            {
                var objectValue = currentContext.First.Value;
                if (objectValue is ObjectHandleSerializer objectHandle)
                    objectValue = objectHandle.Unwrap();
                var newContext = currentContext.Count > 1 ? new LinkedList<object>(currentContext) : null;
                if (newContext != null)
                    newContext.RemoveFirst();
                
                SetNestedContextCallContext(newContext);
                return objectValue;
            }
            return null;
#endif
        }

        [Obsolete("Replaced by ScopeContext.Clear. Marked obsolete on NLog 5.0")]
        internal static void ClearMappedContextLegacy()
        {
#if !NET35 && !NET40 && !NET45
            var contextState = GetAsyncLocalContext();
            if (contextState != null)
            {
                LegacyScopeContext.CaptureLegacyContext(contextState, out var allProperties, out var nestedContext, out var nestedContextTimestamp);
                if (nestedContext?.Length > 0)
                {
                    if (allProperties?.Count > 0)
                    {
                        var legacyScope = new LegacyScopeContext(null, nestedContext, nestedContextTimestamp);
                        SetAsyncLocalContext(legacyScope);
                    }
                }
                else
                {
                    SetAsyncLocalContext(null);
                }
            }
#else
            ClearMappedContextCallContext();
#endif
        }

        [Obsolete("Replaced by ScopeContext.Clear. Marked obsolete on NLog 5.0")]
        internal static void ClearNestedContextLegacy()
        {
#if !NET35 && !NET40 && !NET45
            var contextState = GetAsyncLocalContext();
            if (contextState != null)
            {
                LegacyScopeContext.CaptureLegacyContext(contextState, out var allProperties, out var nestedContext, out var nestedContextTimestamp);
                if (allProperties?.Count > 0)
                {
                    if (nestedContext?.Length > 0)
                    {
                        var legacyScope = new LegacyScopeContext(allProperties, ArrayHelper.Empty<object>(), 0L);
                        SetAsyncLocalContext(legacyScope);
                    }
                }
                else
                {
                    SetAsyncLocalContext(null);
                }
            }
#else
            ClearNestedContextCallContext();
#endif
        }

#if NET35 || NET40 || NET45

#if !NET35 && !NET40
        private static Dictionary<string, object> PushPropertiesCallContext<TValue>(IReadOnlyCollection<KeyValuePair<string, TValue>> properties)
        {
            var oldContext = GetMappedContextCallContext();
            var newContext = CloneMappedContext(oldContext, properties.Count);
            using (var scopeEnumerator = new ScopePropertiesEnumerator<TValue>(properties))
            {
                while (scopeEnumerator.MoveNext())
                {
                    var item = scopeEnumerator.Current;
                    SetPropertyCallContext(item.Key, item.Value, newContext);
                }
            }
            SetMappedContextCallContext(newContext);
            return oldContext;
        }
#endif

        private static Dictionary<string, object> PushPropertyCallContext<TValue>(string propertyName, TValue propertyValue)
        {
            var oldContext = GetMappedContextCallContext();
            var newContext = CloneMappedContext(oldContext, 1);
            SetPropertyCallContext(propertyName, propertyValue, newContext);
            SetMappedContextCallContext(newContext);
            return oldContext;
        }

        private static void ClearMappedContextCallContext()
        {
            SetMappedContextCallContext(null);
        }

        private static IEnumerable<KeyValuePair<string, object>> GetAllPropertiesUnwrapped(Dictionary<string, object> properties)
        {
            foreach (var item in properties)
            {
                if (item.Value is ObjectHandleSerializer objectHandle)
                {
                    yield return new KeyValuePair<string, object>(item.Key, objectHandle.Unwrap());
                }
                else
                {
                    yield return item;
                }
            }
        }

        private static Dictionary<string, object> CloneMappedContext(Dictionary<string, object> oldContext, int initialCapacity = 0)
        {
            if (oldContext?.Count > 0)
            {
                var dictionary = new Dictionary<string, object>(oldContext.Count + initialCapacity, DefaultComparer);
                foreach (var keyValue in oldContext)
                    dictionary[keyValue.Key] = keyValue.Value;
                return dictionary;
            }

            return new Dictionary<string, object>(initialCapacity, DefaultComparer);
        }

        private static void SetPropertyCallContext<TValue>(string item, TValue value, IDictionary<string, object> mappedContext)
        {
            object objectValue = value;
            if (Convert.GetTypeCode(objectValue) != TypeCode.Object)
                mappedContext[item] = objectValue;
            else
                mappedContext[item] = new ObjectHandleSerializer(objectValue);
        }

        private sealed class ScopeContextProperties : IDisposable
        {
            private readonly Dictionary<string, object> _oldContext;
            private bool _diposed;

            public ScopeContextProperties(Dictionary<string, object> oldContext)
            {
                _oldContext = oldContext;
            }

            public void Dispose()
            {
                if (!_diposed)
                {
                    SetMappedContextCallContext(_oldContext);
                    _diposed = true;
                }
            }
        }

        private static void SetMappedContextCallContext(Dictionary<string, object> newValue)
        {
            if (newValue == null)
                System.Runtime.Remoting.Messaging.CallContext.FreeNamedDataSlot(MappedContextDataSlotName);
            else
                System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(MappedContextDataSlotName, newValue);
        }

        internal static Dictionary<string, object> GetMappedContextCallContext()
        {
            return System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(MappedContextDataSlotName) as Dictionary<string, object>;
        }
        
        private const string MappedContextDataSlotName = "NLog.AsyncableMappedDiagnosticsContext";
#endif

#if NET35 || NET40 || NET45

        private static LinkedList<object> PushNestedStateCallContext(object objectValue)
        {
            var oldContext = GetNestedContextCallContext();
            var newContext = oldContext?.Count > 0 ? new LinkedList<object>(oldContext) : new LinkedList<object>();
            if (Convert.GetTypeCode(objectValue) == TypeCode.Object)
                objectValue = new ObjectHandleSerializer(objectValue);
            newContext.AddFirst(objectValue);
            SetNestedContextCallContext(newContext);
            return oldContext;
        }

        private static void ClearNestedContextCallContext()
        {
            SetNestedContextCallContext(null);
        }

        private sealed class ScopeContextNestedState : IDisposable
        {
            private readonly LinkedList<object> _oldContext;
            private readonly object _nestedState;
            private bool _diposed;

            public ScopeContextNestedState(LinkedList<object> oldContext, object nestedState)
            {
                _oldContext = oldContext;
                _nestedState = nestedState;
            }

            public void Dispose()
            {
                if (!_diposed)
                {
                    SetNestedContextCallContext(_oldContext);
                    _diposed = true;
                }
            }

            public override string ToString()
            {
                return _nestedState?.ToString() ?? "null";
            }
        }

        [System.Security.SecuritySafeCriticalAttribute]
        private static void SetNestedContextCallContext(LinkedList<object> nestedContext)
        {
            if (nestedContext == null)
                System.Runtime.Remoting.Messaging.CallContext.FreeNamedDataSlot(NestedContextDataSlotName );
            else
                System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(NestedContextDataSlotName , nestedContext);
        }
        
        [System.Security.SecuritySafeCriticalAttribute]

        private static LinkedList<object> GetNestedContextCallContext()
        {
            return System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(NestedContextDataSlotName ) as LinkedList<object>;
        }

        private const string NestedContextDataSlotName = "NLog.AsyncNestedDiagnosticsLogicalContext";
#endif
    }
}
