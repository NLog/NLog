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
    /// within a scope can include the scope state when wanted. The logical operation scope state supports both
    /// scope-properties and scope-operation-states.
    /// </summary>
    /// <remarks>
    /// <see cref="ScopeContext"/> unifies <see cref="MappedDiagnosticsLogicalContext"/> (MDLC) and <see cref="NestedDiagnosticsLogicalContext"/> (NDLC).
    /// 
    /// .NetCore (and .Net46) uses AsyncLocal for handling the thread execution context. Older .NetFramework uses System.Runtime.Remoting.CallContext
    /// </remarks>
    public static class ScopeContext
    {
#if !NET35 && !NET40
        /// <summary>
        /// Pushes new operation state on the logical context scope operation stack, and includes the provided properties
        /// </summary>
        /// <param name="operationState">Value to added to the scope operation stack</param>
        /// <param name="properties">Properties being added to the scope dictionary</param>
        /// <returns>A disposable object that ends the logical operation scope on dispose.</returns>
        /// <remarks>Scope dictionary keys are case-insensitive</remarks>
        public static IDisposable PushOperationProperties(object operationState, IReadOnlyList<KeyValuePair<string, object>> properties)
        {
#if !NET45
            var parent = GetThreadLocal();
            IScopeContext current = null;
            if (properties?.Count > 0)
                current = new ScopeContextProperties(parent, properties, operationState);
            else 
                current = new ScopeContextOperationState<object>(parent, operationState);
            SetThreadLocal(current);
            return current;
#else
            if (properties?.Count > 0)
            {
                var mldcScope = MappedDiagnosticsLogicalContext.PushProperties(properties);
                var ndlcScope = NestedDiagnosticsLogicalContext.PushOperationState(operationState);
                return new ScopeContextOperationProperties(ndlcScope, mldcScope);
            }
            else
            {
                return NestedDiagnosticsLogicalContext.PushOperationState(operationState);
            }
#endif
        }
#endif

#if !NET35 && !NET40
        /// <summary>
        /// Updates the logical scope context with provided properties
        /// </summary>
        /// <param name="properties">Properties being added to the scope dictionary</param>
        /// <returns>A disposable object that ends the logical operation scope on dispose.</returns>
        /// <remarks>Scope dictionary keys are case-insensitive</remarks>
        public static IDisposable PushProperties(IReadOnlyList<KeyValuePair<string, object>> properties)
        {
#if !NET45
            var parent = GetThreadLocal();
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
        /// <returns>A disposable object that ends the logical operation scope on dispose.</returns>
        /// <remarks>Scope dictionary keys are case-insensitive</remarks>
        public static IDisposable PushProperty<T>(string key, T value)
        {
#if !NET35 && !NET40 && !NET45
            // Skips casting to check for properties
            var parent = GetThreadLocal();
            var current = new ScopeContextProperty<T>(parent, key, value);
            SetThreadLocal(current);
            return current;
#else
            return MappedDiagnosticsLogicalContext.PushProperty(key, value);
#endif
        }

        /// <summary>
        /// Pushes new operation state on the logical context scope operation stack
        /// </summary>
        /// <param name="operationState">Value to added to the scope operation stack</param>
        /// <returns>A disposable object that ends the logical operation scope on dispose.</returns>
        public static IDisposable PushOperationState<T>(T operationState)
        {
#if !NET35 && !NET40 && !NET45
            // Skips casting to check for properties
            var parent = GetThreadLocal();
            var current = new ScopeContextOperationState<T>(parent, operationState);
            SetThreadLocal(current);
            return current;
#else
            return NestedDiagnosticsLogicalContext.PushOperationState(operationState);
#endif
        }

        /// <summary>
        /// Clears all logical operation scopes
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
            return contextState?.CaptureScopeProperties(0, out var _) ?? ArrayHelper.Empty<KeyValuePair<string, object>>();
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
        public static bool TryLookupProperty(string key, out object value)
        {
#if !NET35 && !NET40 && !NET45
            var contextState = GetThreadLocal();
            if (contextState != null)
            {
                var mappedContext = contextState?.CaptureScopeProperties(0, out var _);
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
            return MappedDiagnosticsLogicalContext.TryLookupProperty(key, out value);
#endif
        }

        /// <summary>
        /// Retrieves all operation states inside the logical context scope operation stack
        /// </summary>
        /// <returns>Array of operation state objects.</returns>
        public static object[] GetAllOperationStates()
        {
#if !NET35 && !NET40 && !NET45
            var parent = GetThreadLocal();
            return parent?.CaptureScopeOperationStates(0, out var _) ?? ArrayHelper.Empty<object>();
#else
            return NestedDiagnosticsLogicalContext.GetAllOperationStates();
#endif
        }

        /// <summary>
        /// Peeks the top value from the logical context scope operation stack
        /// </summary>
        /// <returns>Value from the top of the stack.</returns>
        public static object PeekOperationState()
        {
#if !NET35 && !NET40 && !NET45
            var parent = GetThreadLocal();
            while (parent != null)
            {
                var nestedContext = parent.OperationState;
                if (nestedContext != null)
                    return nestedContext;

                parent = parent.Parent;
            }
            return null;
#else
            return NestedDiagnosticsLogicalContext.PeekOperationState();
#endif
        }

        /// <summary>
        /// Peeks the inner operation from the logical context scope operation stack, and returns its running duration
        /// </summary>
        /// <returns>Scope Duration Time</returns>
        internal static TimeSpan? PeekInnerOperationDuration()
        {
#if !NET35 && !NET40 && !NET45
            var stopwatchNow = GetScopeOperationTimestamp(); // Early timestamp to reduce chance of measuring NLog time
            var parent = GetThreadLocal();
            while (parent != null)
            {
                var scopeTimestamp = parent.OperationTimestamp;
                if (scopeTimestamp != 0)
                {
                    return GetScopeOperationDuration(scopeTimestamp, stopwatchNow);
                }

                parent = parent.Parent;
            }
            return null;
#else
            return default(TimeSpan?);  // Delay timing only supported when using AsyncLocal. CallContext is very sensitive.
#endif
        }

        /// <summary>
        /// Peeks the outer operation from the logical context scope operation stack, and returns its running duration
        /// </summary>
        /// <returns>Scope Duration Time</returns>
        internal static TimeSpan? PeekOuterOperationDuration()
        {
#if !NET35 && !NET40 && !NET45
            var stopwatchNow = GetScopeOperationTimestamp(); // Early timestamp to reduce chance of measuring NLog time
            var parent = GetThreadLocal();
            var scopeTimestamp = 0L;
            while (parent != null)
            {
                if (parent.OperationTimestamp != 0)
                    scopeTimestamp = parent.OperationTimestamp;
                parent = parent.Parent;
            }

            if (scopeTimestamp != 0L)
            {
                return GetScopeOperationDuration(scopeTimestamp, stopwatchNow);
            }

            return null;
#else
            return default(TimeSpan?);  // Delay timing only supported when using AsyncLocal. CallContext is very sensitive.
#endif
        }

#if !NET35 && !NET40 && !NET45
        internal static void SetMappedContextLegacy<T>(string key, T value)
        {
            if (TryLookupProperty(key, out var existingValue))
            {
                if (existingValue is IConvertible left
                 && value is IConvertible right
                 && left.GetTypeCode() == right.GetTypeCode()
                 && value.Equals(existingValue))
                    return; // No update is needed

                // Replace with new legacy-scope, the legacy-scope can be discarded when previous parent scope is restored
                var contextState = GetThreadLocal();
                CaptureLegacyContext(contextState, out var scopeDictionary, out var scopeOperationStates, out var scopeOperationTimestamp);
                scopeDictionary[key] = value;

                var legacyScope = new LegacyScopeContext(scopeDictionary, scopeOperationStates, scopeOperationTimestamp);
                SetThreadLocal(legacyScope);
            }
            else
            {
                PushProperty(key, value);
            }
        }

        internal static void RemoveMappedContextLegacy(string key)
        {
            if (TryLookupProperty(key, out var _))
            {
                // Replace with new legacy-scope, the legacy-scope can be discarded when previous parent scope is restored
                var contextState = GetThreadLocal();
                CaptureLegacyContext(contextState, out var scopeDictionary, out var scopeOperationStates, out var scopeOperationTimestamp);
                scopeDictionary.Remove(key);

                var legacyScope = new LegacyScopeContext(scopeDictionary, scopeOperationStates, scopeOperationTimestamp);
                SetThreadLocal(legacyScope);
            }
        }

        internal static object PopNestedContextLegacy()
        {
            var contextState = GetThreadLocal();
            if (contextState != null)
            {
                if ((contextState.Parent == null && contextState is LegacyScopeContext) || contextState.OperationState == null)
                {
                    var scopeOperationStates = contextState?.CaptureScopeOperationStates(0, out var _) ?? ArrayHelper.Empty<object>();
                    if (scopeOperationStates.Length == 0)
                        return null;    // Nothing to pop, just leave scope alone

                    // Replace with new legacy-scope, the legacy-scope can be discarded when previous parent scope is restored
                    var stackTopValue = scopeOperationStates[0];
                    var scopeProperties = contextState?.CaptureScopeProperties(0, out var _) ?? ArrayHelper.Empty<KeyValuePair<string, object>>();
                    if (scopeOperationStates.Length == 1)
                    {
                        scopeOperationStates = ArrayHelper.Empty<object>();
                    }
                    else
                    {
                        var newScope = new object[scopeOperationStates.Length - 1];
                        for (int i = 0; i < newScope.Length; ++i)
                            newScope[i] = scopeOperationStates[i + 1];
                        scopeOperationStates = newScope;
                    }

                    var legacyScope = new LegacyScopeContext(scopeProperties, scopeOperationStates, scopeOperationStates.Length > 0 ? GetScopeOperationTimestamp() : 0L);
                    SetThreadLocal(legacyScope);
                    return stackTopValue;
                }
                else
                {
                    SetThreadLocal(contextState.Parent);
                    return contextState?.OperationState;
                }
            }

            return null;
        }

        internal static void ClearMappedContextLegacy()
        {
            var contextState = GetThreadLocal();
            if (contextState != null)
            {
                CaptureLegacyContext(contextState, out var scopeDictionary, out var scopeOperationStates, out var scopeOperationTimestamp);
                if (scopeOperationStates?.Length > 0)
                {
                    if (scopeDictionary?.Count > 0)
                    {
                        var legacyScope = new LegacyScopeContext(null, scopeOperationStates, scopeOperationTimestamp);
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
                CaptureLegacyContext(contextState, out var scopeDictionary, out var scopeOperationStates, out var scopeOperationTimestamp);
                if (scopeDictionary?.Count > 0)
                {
                    if (scopeOperationStates?.Length > 0)
                    {
                        var legacyScope = new LegacyScopeContext(scopeDictionary, ArrayHelper.Empty<object>(), scopeOperationTimestamp);
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

        private static void CaptureLegacyContext(IScopeContext contextState, out Dictionary<string, object> scopeDictionary, out object[] scopeOperationStates, out long scopeOperationTimestamp)
        {
            scopeOperationStates = contextState?.CaptureScopeOperationStates(0, out var _) ?? ArrayHelper.Empty<object>();
            scopeDictionary = null;
            var scopeProperties = contextState?.CaptureScopeProperties(0, out scopeDictionary) ?? ArrayHelper.Empty<KeyValuePair<string, object>>();
            if (scopeDictionary == null)
            {
                scopeDictionary = new Dictionary<string, object>(DefaultComparer);
                foreach (var scopeProperty in scopeProperties)
                    scopeDictionary[scopeProperty.Key] = scopeProperty.Value;
            }

            scopeOperationTimestamp = 0L;
            if (scopeOperationStates?.Length > 0)
            {
                var parent = contextState;
                while (parent != null)
                {
                    if (parent.OperationTimestamp != 0L)
                        scopeOperationTimestamp = parent.OperationTimestamp;
                    parent = parent.Parent;
                }

                if (scopeOperationTimestamp == 0L)
                    scopeOperationTimestamp = GetScopeOperationTimestamp();
            }
        }

        private static long GetScopeOperationTimestamp()
        {
            if (System.Diagnostics.Stopwatch.IsHighResolution)
                return System.Diagnostics.Stopwatch.GetTimestamp();
            else
                return System.Environment.TickCount;
        }

        private static TimeSpan GetScopeOperationDuration(long scopeTimestamp, long currentTimestamp)
        {
            if (System.Diagnostics.Stopwatch.IsHighResolution)
                return TimeSpan.FromTicks((currentTimestamp - scopeTimestamp) * TimeSpan.TicksPerSecond / System.Diagnostics.Stopwatch.Frequency);
            else
                return TimeSpan.FromMilliseconds((int)currentTimestamp - (int)scopeTimestamp);
        }

        private static Dictionary<string, object> CloneParentContextDictionary(IEnumerable<KeyValuePair<string, object>> parentContext, int accumulatedCount)
        {
            if (parentContext is IReadOnlyList<KeyValuePair<string, object>> parentContextList)
            {
                var scopeProperties = new Dictionary<string, object>(parentContextList.Count + accumulatedCount, DefaultComparer);
                for (int i = 0; i < parentContextList.Count; ++i)
                {
                    var item = parentContextList[i];
                    scopeProperties[item.Key] = item.Value;
                }
                return scopeProperties;
            }
            else if (parentContext is Dictionary<string, object> parentContextDictionary)
            {
                var scopeProperties = new Dictionary<string, object>(parentContextDictionary.Count + accumulatedCount, DefaultComparer);
                foreach (var item in parentContextDictionary)
                {
                    scopeProperties[item.Key] = item.Value;
                }
                return scopeProperties;
            }
            else
            {
                var scopeProperties = new Dictionary<string, object>(1 + accumulatedCount, DefaultComparer);
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
            object OperationState { get; }
            long OperationTimestamp { get; }
            IEnumerable<KeyValuePair<string, object>> CaptureScopeProperties(int accumulatedCount, out Dictionary<string, object> scopeProperties);
            object[] CaptureScopeOperationStates(int accumulatedCount, out object[] scopeOperationStates);
        }

        private sealed class ScopeContextOperationState<T> : IScopeContext
        {
            private readonly T _value;
            private bool _disposed;

            public ScopeContextOperationState(IScopeContext parent, T state)
            {
                Parent = parent;
                OperationTimestamp = GetScopeOperationTimestamp();
                _value = state;
            }

            public IScopeContext Parent { get; }

            object IScopeContext.OperationState => _value;

            public long OperationTimestamp { get; }

            object[] IScopeContext.CaptureScopeOperationStates(int accumulatedCount, out object[] scopeOperationStates)
            {
                scopeOperationStates = null;
                Parent?.CaptureScopeOperationStates(accumulatedCount + 1, out scopeOperationStates);
                if (scopeOperationStates == null)
                    scopeOperationStates = new object[accumulatedCount + 1];
                scopeOperationStates[accumulatedCount] = _value;
                return scopeOperationStates;
            }

            IEnumerable<KeyValuePair<string, object>> IScopeContext.CaptureScopeProperties(int accumulatedCount, out Dictionary<string, object> scopeProperties)
            {
                scopeProperties = null;
                return Parent?.CaptureScopeProperties(accumulatedCount, out scopeProperties) ?? scopeProperties;
            }

            public override string ToString()
            {
                return Parent?.ToString() ?? "null";
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

        private sealed class ScopeContextProperty<T> : IScopeContext
        {
            public IScopeContext Parent { get; }
            long IScopeContext.OperationTimestamp => 0;
            object IScopeContext.OperationState => null;
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

            object[] IScopeContext.CaptureScopeOperationStates(int accumulatedCount, out object[] scopeOperationStates)
            {
                scopeOperationStates = null;
                return Parent?.CaptureScopeOperationStates(accumulatedCount, out scopeOperationStates) ?? ArrayHelper.Empty<object>();
            }

            IEnumerable<KeyValuePair<string, object>> IScopeContext.CaptureScopeProperties(int accumulatedCount, out Dictionary<string, object> scopeProperties)
            {
                scopeProperties = null;
                if (_scopeDictionary != null)
                {
                    return _scopeDictionary;
                }
                else
                {
                    var parentContext = Parent?.CaptureScopeProperties(accumulatedCount + 1, out scopeProperties);
                    if (scopeProperties == null)
                    {
                        if (parentContext == null)
                        {
                            // No more parent-context, build scope-property-collection starting from this scope
                            if (accumulatedCount == 0)
                                return _scopeDictionary = new[] { new KeyValuePair<string, object>(Name, Value) };
                            else
                                scopeProperties = new Dictionary<string, object>(accumulatedCount + 1, DefaultComparer);
                        }
                        else
                        {
                            // Build scope-property-collection from parent-context
                            scopeProperties = CloneParentContextDictionary(parentContext, accumulatedCount + 1);
                        }
                    }

                    scopeProperties[Name] = Value;
                    if (accumulatedCount == 0)
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

        private sealed class ScopeContextProperties : IScopeContext
        {
            public IScopeContext Parent { get; }
            public long OperationTimestamp { get; }
            public object OperationState { get; }
            public IReadOnlyList<KeyValuePair<string, object>> ScopeProperties { get; }
            private IReadOnlyCollection<KeyValuePair<string, object>> _scopeDictionary;
            private bool _disposed;

            public ScopeContextProperties(IScopeContext parent, IReadOnlyList<KeyValuePair<string, object>> scopeProperties, object scopeOperationState)
            {
                Parent = parent;
                ScopeProperties = scopeProperties;
                OperationState = scopeOperationState;
                OperationTimestamp = scopeOperationState != null ? GetScopeOperationTimestamp() : 0;
            }

            object[] IScopeContext.CaptureScopeOperationStates(int accumulatedCount, out object[] scopeOperationStates)
            {
                scopeOperationStates = null;
                int extraCount = (OperationState != null ? 1 : 0);
                Parent?.CaptureScopeOperationStates(accumulatedCount + extraCount, out scopeOperationStates);
                if (extraCount > 0)
                {
                    if (scopeOperationStates == null)
                        scopeOperationStates = new object[accumulatedCount + extraCount];
                    scopeOperationStates[accumulatedCount] = OperationState;
                }
                return scopeOperationStates;
            }

            IEnumerable<KeyValuePair<string, object>> IScopeContext.CaptureScopeProperties(int accumulatedCount, out Dictionary<string, object> scopeProperties)
            {
                scopeProperties = null;
                if (_scopeDictionary != null)
                {
                    return _scopeDictionary;
                }
                else
                {
                    var parentContext = Parent?.CaptureScopeProperties(accumulatedCount + ScopeProperties.Count, out scopeProperties);
                    if (scopeProperties == null)
                    {
                        if (parentContext == null)
                        {
                            // No more parent-context, build scope-property-collection starting from this scope
                            if (accumulatedCount == 0)
                                return _scopeDictionary = EnsureCollectionWithUniqueKeys();
                            else
                                scopeProperties = new Dictionary<string, object>(accumulatedCount + ScopeProperties.Count, DefaultComparer);
                        }
                        else
                        {
                            // Build scope-property-collection from parent-context
                            scopeProperties = CloneParentContextDictionary(parentContext, accumulatedCount + ScopeProperties.Count);
                        }
                    }

                    AppendScopeProperties(scopeProperties);
                    if (accumulatedCount == 0)
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
                return OperationState?.ToString() ?? base.ToString();
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
            public IEnumerable<KeyValuePair<string, object>> MappedContext { get; }
            public long OperationTimestamp { get; }
            private bool _disposed;

            public LegacyScopeContext(IEnumerable<KeyValuePair<string, object>> mappedContext, object[] nestedContext, long scopeOperationTimestamp)
            {
                MappedContext = mappedContext;
                NestedContext = nestedContext;
                OperationTimestamp = scopeOperationTimestamp;
            }

            object IScopeContext.OperationState => NestedContext?.Length > 0 ? NestedContext[0] : null;

            object[] IScopeContext.CaptureScopeOperationStates(int accumulatedCount, out object[] scopeOperationStates)
            {
                scopeOperationStates = null;
                int extraCount = NestedContext?.Length ?? 0;
                Parent?.CaptureScopeOperationStates(accumulatedCount + extraCount, out scopeOperationStates);
                if (extraCount > 0)
                {
                    if (scopeOperationStates == null)
                    {
                        if (accumulatedCount == 0)
                            return NestedContext;
                        else
                            scopeOperationStates = new object[accumulatedCount + extraCount];
                    }

                    for (int i = 0; i < extraCount; ++i)
                        scopeOperationStates[accumulatedCount + i] = NestedContext[i];
                }
                return scopeOperationStates;
            }

            IEnumerable<KeyValuePair<string, object>> IScopeContext.CaptureScopeProperties(int accumulatedCount, out Dictionary<string, object> scopeProperties)
            {
                scopeProperties = null;
                return MappedContext;
            }

            public override string ToString()
            {
                return NestedContext?.ToString() ?? base.ToString();
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
        private sealed class ScopeContextOperationProperties : IDisposable
        {
            private readonly IDisposable _mldcScope;
            private readonly IDisposable _ndlcScope;

            public ScopeContextOperationProperties(IDisposable ndlcScope, IDisposable mldcScope)
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
