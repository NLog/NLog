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

#if !NET35 && !NET40 && !NET45

namespace NLog.Internal
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Immutable state that combines ScopeContext MDLC + NDLC for <see cref="System.Threading.AsyncLocal{T}"/>
    /// </summary>
    internal abstract class ScopeContextAsyncState : IDisposable
    {
        public IScopeContextAsyncState Parent { get; }
        private bool _disposed;

        protected ScopeContextAsyncState(IScopeContextAsyncState parent)
        {
            Parent = parent;
        }

        void IDisposable.Dispose()
        {
            if (!_disposed)
            {
                ScopeContext.SetAsyncLocalContext(Parent);
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Immutable state that combines ScopeContext MDLC + NDLC for <see cref="System.Threading.AsyncLocal{T}"/>
    /// </summary>
    internal interface IScopeContextAsyncState : IDisposable
    {
        IScopeContextAsyncState Parent { get; }
        object NestedState { get; }
        long NestedStateTimestamp { get; }
        IReadOnlyCollection<KeyValuePair<string, object>> CaptureContextProperties(int initialCapacity, out Dictionary<string, object> scopeProperties);
        object[] CaptureNestedContext(int initialCapacity, out object[] nestedContext);
    }

    /// <summary>
    /// Immutable state for ScopeContext Mapped Context (MDLC)
    /// </summary>
    internal interface IScopeContextPropertiesAsyncState : IScopeContextAsyncState
    {
    }

    /// <summary>
    /// Immutable state for ScopeContext Nested State (NDLC)
    /// </summary>
    internal sealed class ScopedContextNestedAsyncState<T> : ScopeContextAsyncState, IScopeContextAsyncState
    {
        private readonly T _value;

        public ScopedContextNestedAsyncState(IScopeContextAsyncState parent, T state)
            :base(parent)
        {
            NestedStateTimestamp = ScopeContext.GetNestedContextTimestampNow();
            _value = state;
        }

        object IScopeContextAsyncState.NestedState => _value;

        public long NestedStateTimestamp { get; }

        object[] IScopeContextAsyncState.CaptureNestedContext(int initialCapacity, out object[] nestedContext)
        {
            nestedContext = null;
            Parent?.CaptureNestedContext(initialCapacity + 1, out nestedContext);
            if (nestedContext is null)
                nestedContext = new object[initialCapacity + 1];
            nestedContext[initialCapacity] = _value;
            return nestedContext;
        }

        IReadOnlyCollection<KeyValuePair<string, object>> IScopeContextAsyncState.CaptureContextProperties(int initialCapacity, out Dictionary<string, object> scopeProperties)
        {
            scopeProperties = null;
            return Parent?.CaptureContextProperties(initialCapacity, out scopeProperties) ?? scopeProperties;
        }

        public override string ToString()
        {
            return _value?.ToString() ?? "null";
        }
    }

    /// <summary>
    /// Immutable state for ScopeContext Single Property (MDLC)
    /// </summary>
    internal sealed class ScopeContextPropertyAsyncState<TValue> : ScopeContextAsyncState, IScopeContextPropertiesAsyncState
    {
        long IScopeContextAsyncState.NestedStateTimestamp => 0;
        object IScopeContextAsyncState.NestedState => null;
        public string Name { get; }
        public TValue Value { get; }
        private IReadOnlyCollection<KeyValuePair<string, object>> _allProperties;

        public ScopeContextPropertyAsyncState(IScopeContextAsyncState parent, string name, TValue value)
            : base(parent)
        {
            Name = name;
            Value = value;
        }

        object[] IScopeContextAsyncState.CaptureNestedContext(int initialCapacity, out object[] nestedContext)
        {
            nestedContext = null;
            return Parent?.CaptureNestedContext(initialCapacity, out nestedContext) ?? ArrayHelper.Empty<object>();
        }

        IReadOnlyCollection<KeyValuePair<string, object>> IScopeContextAsyncState.CaptureContextProperties(int initialCapacity, out Dictionary<string, object> scopeProperties)
        {
            scopeProperties = null;
            if (_allProperties != null)
            {
                return _allProperties;
            }
            else
            {
                var parentContext = Parent?.CaptureContextProperties(initialCapacity + 1, out scopeProperties);
                if (scopeProperties is null)
                {
                    if (parentContext is null)
                    {
                        // No more parent-context, build scope-property-collection starting from this scope
                        if (initialCapacity == 0)
                            return _allProperties = new[] { new KeyValuePair<string, object>(Name, Value) };
                        else
                            scopeProperties = new Dictionary<string, object>(initialCapacity + 1, ScopeContext.DefaultComparer);
                    }
                    else
                    {
                        // Build scope-property-collection from parent-context
                        scopeProperties = ScopeContextPropertyEnumerator<object>.CloneScopePropertiesToDictionary(parentContext, initialCapacity + 1);
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
    }

    /// <summary>
    /// Immutable state for ScopeContext Multiple Properties (MDLC)
    /// </summary>
    internal sealed class ScopeContextPropertiesAsyncState<TValue> : ScopeContextAsyncState, IScopeContextPropertiesAsyncState, IReadOnlyCollection<KeyValuePair<string, object>>
    {
        public long NestedStateTimestamp { get; }
        public object NestedState { get; }

        private readonly IReadOnlyCollection<KeyValuePair<string, TValue>> _scopeProperties;
        private IReadOnlyCollection<KeyValuePair<string, object>> _allProperties;

        public ScopeContextPropertiesAsyncState(IScopeContextAsyncState parent, Dictionary<string, object> allProperties)
            :base(parent)
        {
            _allProperties = allProperties; // Collapsed dictionary that includes all properties from parent scopes with case-insensitive-comparer
        }

        public ScopeContextPropertiesAsyncState(IScopeContextAsyncState parent, IReadOnlyCollection<KeyValuePair<string, TValue>> scopeProperties, object nestedState)
            :base(parent)
        {
            _scopeProperties = scopeProperties;
            NestedState = nestedState;
            NestedStateTimestamp = nestedState is null ? 0 : ScopeContext.GetNestedContextTimestampNow();
        }

        object[] IScopeContextAsyncState.CaptureNestedContext(int initialCapacity, out object[] nestedContext)
        {
            nestedContext = null;
            int extraCount = NestedState is null ? 0 : 1;
            Parent?.CaptureNestedContext(initialCapacity + extraCount, out nestedContext);
            if (extraCount > 0)
            {
                if (nestedContext is null)
                    nestedContext = new object[initialCapacity + extraCount];
                nestedContext[initialCapacity] = NestedState;
            }
            return nestedContext;
        }

        IReadOnlyCollection<KeyValuePair<string, object>> IScopeContextAsyncState.CaptureContextProperties(int initialCapacity, out Dictionary<string, object> scopeProperties)
        {
            scopeProperties = null;
            if (_allProperties != null)
            {
                return _allProperties;
            }
            else
            {
                var parentContext = Parent?.CaptureContextProperties(initialCapacity + _scopeProperties.Count, out scopeProperties);
                if (scopeProperties is null)
                {
                    if (parentContext is null)
                    {
                        // No more parent-context, build scope-property-collection starting from this scope
                        if (initialCapacity == 0)
                            return _allProperties = EnsureCollectionWithUniqueKeys();
                        else
                            scopeProperties = new Dictionary<string, object>(initialCapacity + _scopeProperties.Count, ScopeContext.DefaultComparer);
                    }
                    else
                    {
                        // Build scope-property-collection from parent-context
                        scopeProperties = ScopeContextPropertyEnumerator<object>.CloneScopePropertiesToDictionary(parentContext, initialCapacity + _scopeProperties.Count);
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
                var scopeDictionary = new Dictionary<string, object>(_scopeProperties.Count, ScopeContext.DefaultComparer);
                AppendScopeProperties(scopeDictionary);
                return scopeDictionary;
            }

            if (propertyCount > 1 && !ScopeContextPropertyEnumerator<TValue>.HasUniqueCollectionKeys(_scopeProperties, ScopeContext.DefaultComparer))
            {
                var scopeDictionary = new Dictionary<string, object>(_scopeProperties.Count, ScopeContext.DefaultComparer);
                AppendScopeProperties(scopeDictionary);
                return scopeDictionary;
            }

            return _scopeProperties as IReadOnlyCollection<KeyValuePair<string, object>> ?? this;
        }

        private void AppendScopeProperties(Dictionary<string, object> scopeDictionary)
        {
            ScopeContextPropertyEnumerator<TValue>.CopyScopePropertiesToDictionary(_scopeProperties, scopeDictionary);
        }

        public override string ToString()
        {
            return NestedState?.ToString() ?? $"Count = {Count}";
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

    /// <summary>
    /// Immutable state for ScopeContext handling legacy MDLC + NDLC operations
    /// </summary>
    [Obsolete("Replaced by ScopeContext.PushProperty / ScopeContext.PushNestedState")]
    internal sealed class ScopeContextLegacyAsyncState : ScopeContextAsyncState, IScopeContextAsyncState
    {
        public object[] NestedContext { get; }
        public IReadOnlyCollection<KeyValuePair<string, object>> MappedContext { get; }
        public long NestedStateTimestamp { get; }

        public ScopeContextLegacyAsyncState(IReadOnlyCollection<KeyValuePair<string, object>> allProperties, object[] nestedContext, long nestedContextTimestamp)
            :base(null) // Always top parent
        {
            MappedContext = allProperties;
            NestedContext = nestedContext;
            NestedStateTimestamp = nestedContextTimestamp;
        }

        public static void CaptureLegacyContext(IScopeContextAsyncState contextState, out Dictionary<string, object> allProperties, out object[] nestedContext, out long nestedContextTimestamp)
        {
            nestedContext = contextState?.CaptureNestedContext(0, out var _) ?? ArrayHelper.Empty<object>();
            allProperties = null;
            var scopeProperties = contextState?.CaptureContextProperties(0, out allProperties) ?? ArrayHelper.Empty<KeyValuePair<string, object>>();
            if (allProperties is null)
            {
                allProperties = new Dictionary<string, object>(scopeProperties.Count, ScopeContext.DefaultComparer);
                ScopeContextPropertyEnumerator<object>.CopyScopePropertiesToDictionary(scopeProperties, allProperties);
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
                    nestedContextTimestamp = ScopeContext.GetNestedContextTimestampNow();
            }
        }

        object IScopeContextAsyncState.NestedState => NestedContext?.Length > 0 ? NestedContext[0] : null;

        object[] IScopeContextAsyncState.CaptureNestedContext(int initialCapacity, out object[] nestedContext)
        {
            nestedContext = null;
            int extraCount = NestedContext?.Length ?? 0;
            Parent?.CaptureNestedContext(initialCapacity + extraCount, out nestedContext);
            if (extraCount > 0)
            {
                if (nestedContext is null)
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

        IReadOnlyCollection<KeyValuePair<string, object>> IScopeContextAsyncState.CaptureContextProperties(int initialCapacity, out Dictionary<string, object> scopeProperties)
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
    }
}

#endif