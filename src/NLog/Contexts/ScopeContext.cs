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

using System;
using System.Collections.Generic;
using System.Linq;
using NLog.Internal;

namespace NLog
{
    /// <summary>
    /// <see cref="ScopeContext"/> stores state in the async thread execution context. All LogEvents created
    /// within a scope can include the scope state in the target output. The logical context scope supports
    /// both scope-properties and scope-nested-state-stack (Similar to log4j2 ThreadContext)
    /// </summary>
    /// <remarks>
    /// <see cref="MappedDiagnosticsLogicalContext"/> (MDLC), <see cref="MappedDiagnosticsContext"/> (MDC), <see cref="NestedDiagnosticsLogicalContext"/> (NDLC)
    /// and <see cref="NestedDiagnosticsContext"/> (NDC) have been deprecated and replaced by <see cref="ScopeContext"/>.
    /// 
    /// .NetCore (and .Net46) uses AsyncLocal for handling the thread execution context. Older .NetFramework uses System.Runtime.Remoting.CallContext
    /// </remarks>
    public static class ScopeContext
    {
        internal static readonly IEqualityComparer<string> DefaultComparer = StringComparer.OrdinalIgnoreCase;

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
            properties = properties ?? ArrayHelper.Empty<KeyValuePair<string, object>>();
            if (properties.Count > 0 || nestedState is null)
            {
#if !NET45
                var parent = GetAsyncLocalContext();
                if (nestedState is null)
                {
                    var allProperties = ScopeContextPropertiesCollapsed.BuildCollapsedDictionary(parent, properties.Count);
                    if (allProperties != null)
                    {
                        // Collapse all 3 property-scopes into a collapsed scope, and return bookmark that can restore original parent (Avoid huge object-graphs)
                        ScopeContextPropertyEnumerator<object>.CopyScopePropertiesToDictionary(properties, allProperties);

                        var collapsedState = new ScopeContextPropertiesAsyncState<object>(parent.Parent.Parent, allProperties, nestedState);
                        SetAsyncLocalContext(collapsedState);
                        return new ScopeContextPropertiesCollapsed(parent, collapsedState);
                    }
                }

                var current = new ScopeContextPropertiesAsyncState<object>(parent, properties, nestedState);
                SetAsyncLocalContext(current);
                return current;
#else
                var oldMappedContext = PushPropertiesCallContext(properties);
                var oldNestedContext = nestedState is null ? null : PushNestedStateCallContext(nestedState);
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
                ScopeContextPropertyEnumerator<TValue>.CopyScopePropertiesToDictionary(properties, allProperties);

                var collapsedState = new ScopeContextPropertiesAsyncState<object>(parent.Parent.Parent, allProperties);
                SetAsyncLocalContext(collapsedState);
                return new ScopeContextPropertiesCollapsed(parent, collapsedState);
            }

            var current = new ScopeContextPropertiesAsyncState<TValue>(parent, properties);
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

            var allProperties = ScopeContextPropertiesCollapsed.BuildCollapsedDictionary(parent, 1);
            if (allProperties != null)
            {
                // Collapse all 3 property-scopes into a collapsed scope, and return bookmark that can restore original parent (Avoid huge object-graphs)
                allProperties[key] = value;

                var collapsedState = new ScopeContextPropertiesAsyncState<object>(parent.Parent.Parent, allProperties);
                SetAsyncLocalContext(collapsedState);
                return new ScopeContextPropertiesCollapsed(parent, collapsedState);
            }

            var current = new ScopeContextPropertyAsyncState<TValue>(parent, key, value);
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
            var current = new ScopedContextNestedAsyncState<T>(parent, nestedState);
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
            var propertyCollector = new ScopeContextPropertyCollector();
            return contextState?.CaptureContextProperties(ref propertyCollector) ?? ArrayHelper.Empty<KeyValuePair<string, object>>();
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

        internal static ScopeContextPropertyEnumerator<object> GetAllPropertiesEnumerator()
        {
            return new ScopeContextPropertyEnumerator<object>(GetAllProperties());
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
                var propertyCollector = new ScopeContextPropertyCollector();
                var mappedContext = contextState.CaptureContextProperties(ref propertyCollector);
                if (mappedContext?.Count > 0)
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
            var nestedStates = GetAllNestedStateList();
            if (nestedStates?.Count > 0)
            {
                if (nestedStates is object[] nestedArray)
                    return nestedArray;
                else
                    return Enumerable.ToArray(nestedStates);
            }
            return ArrayHelper.Empty<object>();
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

#if !NET35 && !NET40 && !NET45
        internal static IList<object> GetAllNestedStateList()
        {
            var parent = GetAsyncLocalContext();
            var nestedStateCollector = new ScopeContextNestedStateCollector();
            return parent?.CaptureNestedContext(ref nestedStateCollector) ?? ArrayHelper.Empty<object>();
        }
#else
        internal static IList<object> GetAllNestedStateList()
        {
            return GetAllNestedStates();
        }
#endif

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
                using (var scopeEnumerator = new ScopeContextPropertyEnumerator<object>(scopeProperties))
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

        internal static long GetNestedContextTimestampNow()
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

        /// <summary>
        /// Special bookmark that can restore original parent, after scopes has been collapsed
        /// </summary>
        private sealed class ScopeContextPropertiesCollapsed : IDisposable
        {
            private readonly IScopeContextAsyncState _parent;
            private readonly IScopeContextPropertiesAsyncState _collapsed;
            private bool _disposed;

            public ScopeContextPropertiesCollapsed(IScopeContextAsyncState parent, IScopeContextPropertiesAsyncState collapsed)
            {
                _parent = parent;
                _collapsed = collapsed;
            }

            public static Dictionary<string, object> BuildCollapsedDictionary(IScopeContextAsyncState parent, int initialCapacity)
            {
                if (parent is IScopeContextPropertiesAsyncState parentProperties && parentProperties.Parent is IScopeContextPropertiesAsyncState grandParentProperties)
                {
                    if (parentProperties.NestedState is null && grandParentProperties.NestedState is null)
                    {
                        var propertyCollectorList = new List<KeyValuePair<string, object>>();   // Marks the collector as active
                        var propertyCollector = new ScopeContextPropertyCollector(propertyCollectorList);
                        var propertyCollection = propertyCollector.StartCaptureProperties(parent);
                        if (propertyCollectorList.Count > 0 && propertyCollection is Dictionary<string, object> propertyDictionary)
                            return propertyDictionary;  // New property collector was built from the list
                        propertyDictionary = new Dictionary<string, object>(propertyCollection.Count + initialCapacity, ScopeContext.DefaultComparer);
                        ScopeContextPropertyEnumerator<object>.CopyScopePropertiesToDictionary(propertyCollection, propertyDictionary);
                        return propertyDictionary;
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

        internal static void SetAsyncLocalContext(IScopeContextAsyncState newValue)
        {
            AsyncNestedDiagnosticsContext.Value = newValue;
        }

        private static IScopeContextAsyncState GetAsyncLocalContext()
        {
            return AsyncNestedDiagnosticsContext.Value;
        }


        private static readonly System.Threading.AsyncLocal<IScopeContextAsyncState> AsyncNestedDiagnosticsContext = new System.Threading.AsyncLocal<IScopeContextAsyncState>();
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
                if (_parentNestedContext != null)
                    SetNestedContextCallContext(_parentNestedContext);
                SetMappedContextCallContext(_parentMappedContext);
            }
        }
#endif


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
            var contextState = GetAsyncLocalContext();
            var propertyCollector = new ScopeContextPropertyCollector();
            var scopeProperties = contextState?.CaptureContextProperties(ref propertyCollector);
            if (scopeProperties?.Count > 0)
            {
                if (scopeProperties.Count == 1)
                    return new[] { Enumerable.First(scopeProperties).Key };
                else if (scopeProperties is IDictionary<string, object> dictionary)
                    return dictionary.Keys;
                else
                    return scopeProperties.Select(prop => prop.Key).ToList();
            }
            return ArrayHelper.Empty<string>();
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
                ScopeContextLegacyAsyncState.CaptureLegacyContext(contextState, out var allProperties, out var scopeNestedStates, out var scopeNestedStateTimestamp);
                allProperties.Remove(key);

                var legacyScope = new ScopeContextLegacyAsyncState(allProperties, scopeNestedStates, scopeNestedStateTimestamp);
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
                if ((contextState.Parent is null && contextState is ScopeContextLegacyAsyncState) || contextState.NestedState is null)
                {
                    var nestedStateCollector = new ScopeContextNestedStateCollector();
                    var nestedStates = contextState.CaptureNestedContext(ref nestedStateCollector) ?? ArrayHelper.Empty<object>();
                    if (nestedStates.Count == 0)
                        return null;    // Nothing to pop, just leave scope alone

                    // Replace with new legacy-scope, the legacy-scope can be discarded when previous parent scope is restored
                    var propertyCollector = new ScopeContextPropertyCollector();
                    var stackTopValue = nestedStates[0];
                    var allProperties = contextState.CaptureContextProperties(ref propertyCollector) ?? ArrayHelper.Empty<KeyValuePair<string, object>>();
                    var nestedContext = ArrayHelper.Empty<object>();
                    if (nestedStates.Count > 1)
                    {
                        nestedContext = new object[nestedStates.Count - 1];
                        for (int i = 0; i < nestedContext.Length; ++i)
                            nestedContext[i] = nestedStates[i + 1];
                    }

                    var legacyScope = new ScopeContextLegacyAsyncState(allProperties, nestedContext, nestedContext.Length > 0 ? GetNestedContextTimestampNow() : 0L);
                    SetAsyncLocalContext(legacyScope);
                    return stackTopValue;
                }
                else
                {
                    SetAsyncLocalContext(contextState.Parent);
                    return contextState.NestedState;
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
                ScopeContextLegacyAsyncState.CaptureLegacyContext(contextState, out var allProperties, out var nestedContext, out var nestedContextTimestamp);
                if (nestedContext?.Length > 0)
                {
                    if (allProperties?.Count > 0)
                    {
                        var legacyScope = new ScopeContextLegacyAsyncState(null, nestedContext, nestedContextTimestamp);
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
                ScopeContextLegacyAsyncState.CaptureLegacyContext(contextState, out var allProperties, out var nestedContext, out var nestedContextTimestamp);
                if (allProperties?.Count > 0)
                {
                    if (nestedContext?.Length > 0)
                    {
                        var legacyScope = new ScopeContextLegacyAsyncState(allProperties, ArrayHelper.Empty<object>(), 0L);
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
            using (var scopeEnumerator = new ScopeContextPropertyEnumerator<TValue>(properties))
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
            if (newValue is null)
                System.Runtime.Remoting.Messaging.CallContext.FreeNamedDataSlot(MappedContextDataSlotName);
            else
                System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(MappedContextDataSlotName, newValue);
        }

        internal static Dictionary<string, object> GetMappedContextCallContext()
        {
            return System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(MappedContextDataSlotName) as Dictionary<string, object>;
        }
        
        private const string MappedContextDataSlotName = "NLog.AsyncableMappedDiagnosticsContext";

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
            if (nestedContext is null)
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
