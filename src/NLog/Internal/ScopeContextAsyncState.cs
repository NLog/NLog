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

#if !NET35 && !NET40 && !NET45

namespace NLog.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Immutable state that combines ScopeContext MDLC + NDLC for <see cref="System.Threading.AsyncLocal{T}"/>
    /// </summary>
    internal abstract class ScopeContextAsyncState : IDisposable
    {
        public IScopeContextAsyncState? Parent { get; }
        private bool _disposed;

        protected ScopeContextAsyncState(IScopeContextAsyncState? parent)
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
        IScopeContextAsyncState? Parent { get; }
        object? NestedState { get; }
        long NestedStateTimestamp { get; }
        IReadOnlyCollection<KeyValuePair<string, object?>>? CaptureContextProperties(ref ScopeContextPropertyCollector contextCollector);
        IList<object>? CloneNestedContext(ref ScopeContextNestedStateCollector contextCollector);
    }

    struct ScopeContextPropertyCollector
    {
        IReadOnlyCollection<KeyValuePair<string, object?>>? _allProperties;
        ICollection<KeyValuePair<string, object?>>? _propertyCollector;

        public bool IsCollectorEmpty => _allProperties is null || (_allProperties.Count == 0 && _propertyCollector is null);

        public bool IsCollectorInactive => _allProperties is null;

        public ScopeContextPropertyCollector(IReadOnlyCollection<KeyValuePair<string, object?>> allProperties, ICollection<KeyValuePair<string, object?>> propertyCollector)
        {
            _allProperties = allProperties;
            _propertyCollector = propertyCollector;
        }

        public IReadOnlyCollection<KeyValuePair<string, object?>> StartCaptureProperties(IScopeContextAsyncState? state)
        {
            while (state != null)
            {
                var result = state.CaptureContextProperties(ref this);
                if (result != null)
                    return result;
                state = state.Parent;
            }

            return CaptureCompleted(null);
        }

        public IReadOnlyCollection<KeyValuePair<string, object?>> CaptureCompleted(IReadOnlyCollection<KeyValuePair<string, object?>>? properties)
        {
            if (_allProperties?.Count > 0)
            {
                if (properties?.Count > 0)
                {
                    if (_propertyCollector is null)
                    {
                        return _allProperties = MergeUniqueProperties(_allProperties, properties);
                    }

                    AddProperties(properties);
                }

                return _allProperties = EnsureUniqueProperties(_allProperties);
            }
            else
            {
                if (properties?.Count > 0)
                    return _allProperties = EnsureUniqueProperties(properties);
                else
                    return _allProperties = Array.Empty<KeyValuePair<string, object?>>();
            }
        }

        private static Dictionary<string, object?> MergeUniqueProperties(IReadOnlyCollection<KeyValuePair<string, object?>> newProperties, IReadOnlyCollection<KeyValuePair<string, object?>> existingProperties)
        {
            var scopeProperties = new Dictionary<string, object?>(newProperties.Count + existingProperties.Count, ScopeContext.DefaultComparer);
            ScopeContextPropertyEnumerator<object>.CopyScopePropertiesToDictionary(newProperties, scopeProperties);
            ScopeContextPropertyEnumerator<object>.TryAddScopePropertiesToDictionary(existingProperties, scopeProperties);
            return scopeProperties;
        }

        private static IReadOnlyCollection<KeyValuePair<string, object?>> EnsureUniqueProperties(IReadOnlyCollection<KeyValuePair<string, object?>> properties)
        {
            var scopePropertyCount = properties.Count;
            if (scopePropertyCount <= 1)
                return properties;

            if (properties is Dictionary<string, object?> dictionary && ReferenceEquals(dictionary.Comparer, ScopeContext.DefaultComparer))
                return properties;

            // Must validate that collected properties are unique
            if (scopePropertyCount > 10 || !ScopeContextPropertyEnumerator<object?>.HasUniqueCollectionKeys(properties, ScopeContext.DefaultComparer))
            {
                // The newest properties are added first, so do not overwrite with old values
                var scopeProperties = new Dictionary<string, object?>(Math.Min(scopePropertyCount, 10), ScopeContext.DefaultComparer);
                ScopeContextPropertyEnumerator<object>.TryAddScopePropertiesToDictionary(properties, scopeProperties);
                return scopeProperties;
            }

            return properties;
        }

        public void AddProperty(string propertyName, object? propertyValue)
        {
            if (_allProperties is null || IsCollectorEmpty)
            {
                _allProperties = new[] { new KeyValuePair<string, object?>(propertyName, propertyValue) };
            }
            else
            {
                if (_propertyCollector is null)
                {
                    var propertyCollector = new List<KeyValuePair<string, object?>>(Math.Max(4, _allProperties.Count + 1));
                    _propertyCollector = propertyCollector;
                    CollectProperties(_allProperties, propertyCollector);
                    propertyCollector.Add(new KeyValuePair<string, object?>(propertyName, propertyValue));
                    _allProperties = propertyCollector;
                }
                else
                {
                    _propertyCollector.Add(new KeyValuePair<string, object?>(propertyName, propertyValue));
                }
            }
        }

        public void AddProperties(IReadOnlyCollection<KeyValuePair<string, object?>> properties)
        {
            if (_allProperties is null || IsCollectorEmpty)
            {
                _allProperties = properties;
            }
            else if (properties?.Count > 0)
            {
                if (_propertyCollector is null)
                {
                    var propertyCollector = new List<KeyValuePair<string, object?>>(Math.Max(4, _allProperties.Count + properties.Count));
                    _propertyCollector = propertyCollector;
                    CollectProperties(_allProperties, propertyCollector);
                    CollectProperties(properties, propertyCollector);
                    _allProperties = propertyCollector;
                }
                else
                {
                    CollectProperties(properties, _propertyCollector);
                }
            }
        }

        private static void CollectProperties(IReadOnlyCollection<KeyValuePair<string, object?>> properties, ICollection<KeyValuePair<string, object?>> propertyCollector)
        {
            using (var scopeEnumerator = new ScopeContextPropertyEnumerator<object?>(properties))
            {
                while (scopeEnumerator.MoveNext())
                {
                    var property = scopeEnumerator.Current;
                    propertyCollector.Add(property);
                }
            }
        }
    }

    struct ScopeContextNestedStateCollector
    {
        private IList<object> _allNestedStates;

        public bool IsCollectorEmpty => _allNestedStates is null || _allNestedStates.Count == 0;

        public bool IsCollectorInactive => _allNestedStates is null;

        public IList<object> StartCloneNestedContext(IScopeContextAsyncState? state)
        {
            _allNestedStates = _allNestedStates ?? Array.Empty<object>();

            while (state != null)
            {
                var result = state.CloneNestedContext(ref this);
                if (result != null)
                    return result;
                state = state.Parent;
            }

            return _allNestedStates;
        }

        public void CollectNestedState(object state)
        {
            if (_allNestedStates is null || _allNestedStates.Count == 0)
            {
                _allNestedStates = new List<object>();
            }
            _allNestedStates.Add(state);    // Collected in "reversed" order
        }

        public IList<object>? CollectNestedStates(object? nestedState, IScopeContextAsyncState? parent)
        {
            if (IsCollectorInactive && parent is not null)
            {
                if (nestedState is not null)
                    CollectNestedState(nestedState);
                return StartCloneNestedContext(parent);
            }

            if (parent is null && IsCollectorEmpty)
                return nestedState is null ? Array.Empty<object>() : new object[] { nestedState };   // We are done

            if (nestedState is not null)
                CollectNestedState(nestedState);
            return null;    // Continue with Parent
        }
    }

    /// <summary>
    /// Immutable state for ScopeContext Mapped Context (MDLC)
    /// </summary>
    internal interface IScopeContextPropertiesAsyncState : IScopeContextAsyncState
    {
        int PropertyCount { get; }
    }

    /// <summary>
    /// Immutable state for ScopeContext Nested State (NDLC)
    /// </summary>
    internal sealed class ScopedContextNestedAsyncState<T> : ScopeContextAsyncState, IScopeContextAsyncState
    {
        private readonly T _value;

        public ScopedContextNestedAsyncState(IScopeContextAsyncState? parent, T state)
            : base(parent)
        {
            NestedStateTimestamp = ScopeContext.GetNestedContextTimestampNow();
            _value = state;
        }

        object? IScopeContextAsyncState.NestedState => _value;

        public long NestedStateTimestamp { get; }

        IList<object>? IScopeContextAsyncState.CloneNestedContext(ref ScopeContextNestedStateCollector contextCollector)
        {
            object? objectValue = _value;
            return contextCollector.CollectNestedStates(objectValue, Parent);
        }

        IReadOnlyCollection<KeyValuePair<string, object?>>? IScopeContextAsyncState.CaptureContextProperties(ref ScopeContextPropertyCollector contextCollector)
        {
            if (contextCollector.IsCollectorInactive && Parent is not null)
            {
                contextCollector.AddProperties(Array.Empty<KeyValuePair<string, object?>>());    // Mark as active
                return contextCollector.StartCaptureProperties(Parent);   // Start parent enumeration
            }

            return null;    // Continue with Parent
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
        object? IScopeContextAsyncState.NestedState => null;
        int IScopeContextPropertiesAsyncState.PropertyCount => _allProperties?.Count ?? 1;
        public string Name { get; }
        public TValue? Value { get; }
        private IReadOnlyCollection<KeyValuePair<string, object?>>? _allProperties;

        public ScopeContextPropertyAsyncState(IScopeContextAsyncState? parent, string name, TValue? value)
            : base(parent)
        {
            Name = name;
            Value = value;
        }

        IList<object>? IScopeContextAsyncState.CloneNestedContext(ref ScopeContextNestedStateCollector contextCollector)
        {
            return contextCollector.CollectNestedStates(null, Parent);
        }

        IReadOnlyCollection<KeyValuePair<string, object?>>? IScopeContextAsyncState.CaptureContextProperties(ref ScopeContextPropertyCollector contextCollector)
        {
            if (contextCollector.IsCollectorEmpty)
            {
                if (_allProperties is null)
                {
                    contextCollector.AddProperty(Name, Value);
                    _allProperties = contextCollector.StartCaptureProperties(Parent); // Capture all properties from parents
                }
                return _allProperties;  // We are done
            }

            if (_allProperties is null)
            {
                contextCollector.AddProperty(Name, Value);
                return null;    // Continue with Parent
            }

            return contextCollector.CaptureCompleted(_allProperties);     // We are done
        }

        public override string ToString()
        {
            return $"{Name}={Value?.ToString() ?? "null"}";
        }
    }

    /// <summary>
    /// Immutable state for ScopeContext Multiple Properties (MDLC)
    /// </summary>
    internal sealed class ScopeContextMergeAsyncState : ScopeContextAsyncState, IScopeContextPropertiesAsyncState, ICollection<KeyValuePair<string, object?>>, IReadOnlyCollection<KeyValuePair<string, object?>>
    {
        int IScopeContextPropertiesAsyncState.PropertyCount => MergedProperties.Count;

        public long NestedStateTimestamp { get; }
        public object? NestedState { get; }
        public Dictionary<string, object?> MergedProperties { get; }

        public int Count => MergedProperties.Count;

        public ScopeContextMergeAsyncState(IScopeContextAsyncState? parent, int initialCapacity)
          : base(parent)
        {
            MergedProperties = new Dictionary<string, object?>(initialCapacity, ScopeContext.DefaultComparer);
        }

        public ScopeContextMergeAsyncState(IScopeContextAsyncState? parent, int initialCapacity, object? nestedState)
            : base(parent)
        {
            MergedProperties = new Dictionary<string, object?>(initialCapacity, ScopeContext.DefaultComparer);
            NestedState = nestedState;
            NestedStateTimestamp = ScopeContext.GetNestedContextTimestampNow();
        }

        public IReadOnlyCollection<KeyValuePair<string, object?>>? CaptureContextProperties(ref ScopeContextPropertyCollector contextCollector)
        {
            return contextCollector.CaptureCompleted(MergedProperties);     // We are done
        }

        public IList<object>? CloneNestedContext(ref ScopeContextNestedStateCollector contextCollector)
        {
            return contextCollector.CollectNestedStates(NestedState, Parent);
        }

        public void Add(KeyValuePair<string, object?> item)
        {
            // Newest properties are added first, so do not overwrite with old values
#if NETSTANDARD2_1_OR_GREATER || NET
            MergedProperties.TryAdd(item.Key, item.Value);
#else
            if (!MergedProperties.ContainsKey(item.Key))
                MergedProperties.Add(item.Key, item.Value);
#endif
        }

        void ICollection<KeyValuePair<string, object?>>.Clear()
        {
            MergedProperties.Clear();
        }

        bool ICollection<KeyValuePair<string, object?>>.Contains(KeyValuePair<string, object?> item)
        {
            return MergedProperties.ContainsKey(item.Key);
        }

        void ICollection<KeyValuePair<string, object?>>.CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, object?>>)MergedProperties).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, object?>>.Remove(KeyValuePair<string, object?> item)
        {
            return MergedProperties.Remove(item.Key);
        }

        bool ICollection<KeyValuePair<string, object?>>.IsReadOnly => ((ICollection<KeyValuePair<string, object?>>)MergedProperties).IsReadOnly;

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            return MergedProperties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)MergedProperties).GetEnumerator();
        }
    }

    /// <summary>
    /// Immutable state for ScopeContext Multiple Properties (MDLC)
    /// </summary>
    internal sealed class ScopeContextPropertiesAsyncState<TValue> : ScopeContextAsyncState, IScopeContextPropertiesAsyncState, IReadOnlyCollection<KeyValuePair<string, object?>>
    {
        int IScopeContextPropertiesAsyncState.PropertyCount => _allProperties?.Count ?? _scopeProperties.Count;

        public long NestedStateTimestamp { get; }
        public object? NestedState { get; }

        private readonly IReadOnlyCollection<KeyValuePair<string, TValue?>> _scopeProperties;
        private IReadOnlyCollection<KeyValuePair<string, object?>>? _allProperties;

        public ScopeContextPropertiesAsyncState(IScopeContextAsyncState? parent, IReadOnlyCollection<KeyValuePair<string, TValue?>> scopeProperties)
            : base(parent)
        {
            _scopeProperties = scopeProperties;
        }

        public ScopeContextPropertiesAsyncState(IScopeContextAsyncState? parent, IReadOnlyCollection<KeyValuePair<string, TValue?>> scopeProperties, object? nestedState)
            : base(parent)
        {
            _scopeProperties = scopeProperties;
            NestedState = nestedState;
            NestedStateTimestamp = ScopeContext.GetNestedContextTimestampNow();
        }

        IList<object>? IScopeContextAsyncState.CloneNestedContext(ref ScopeContextNestedStateCollector contextCollector)
        {
            return contextCollector.CollectNestedStates(NestedState, Parent);
        }

        IReadOnlyCollection<KeyValuePair<string, object?>>? IScopeContextAsyncState.CaptureContextProperties(ref ScopeContextPropertyCollector contextCollector)
        {
            if (contextCollector.IsCollectorEmpty)
            {
                if (_allProperties is null)
                {
                    contextCollector.AddProperties(_scopeProperties as IReadOnlyCollection<KeyValuePair<string, object?>> ?? this);
                    _allProperties = contextCollector.StartCaptureProperties(Parent); // Capture all properties from parents
                }
                return _allProperties;  // We are done
            }

            if (_allProperties is null)
            {
                contextCollector.AddProperties(_scopeProperties as IReadOnlyCollection<KeyValuePair<string, object?>> ?? this);
                return null;    // Continue with Parent
            }

            return contextCollector.CaptureCompleted(_allProperties);     // We are done
        }

        public override string ToString()
        {
            return NestedState?.ToString() ?? $"Count = {Count}";
        }

        public int Count => _scopeProperties.Count;

        IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator() => new ScopeContextPropertyEnumerator<TValue?>(_scopeProperties);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new ScopeContextPropertyEnumerator<TValue?>(_scopeProperties);
    }

    /// <summary>
    /// Immutable state for ScopeContext handling legacy MDLC + NDLC operations
    /// </summary>
    [Obsolete("Replaced by ScopeContext.PushProperty / ScopeContext.PushNestedState")]
    internal sealed class ScopeContextLegacyAsyncState : ScopeContextAsyncState, IScopeContextAsyncState
    {
        public object[] NestedContext { get; }
        public IReadOnlyCollection<KeyValuePair<string, object?>>? MappedContext { get; }
        public long NestedStateTimestamp { get; }

        public ScopeContextLegacyAsyncState(IReadOnlyCollection<KeyValuePair<string, object?>>? allProperties, object[] nestedContext, long nestedContextTimestamp)
            : base(null) // Always top parent
        {
            MappedContext = allProperties;
            NestedContext = nestedContext;
            NestedStateTimestamp = nestedContextTimestamp;
        }

        public static void CaptureLegacyContext(IScopeContextAsyncState? contextState, out Dictionary<string, object?> allProperties, out object[] nestedContext, out long nestedContextTimestamp)
        {
            var nestedStateCollector = new ScopeContextNestedStateCollector();
            var propertyCollector = new ScopeContextPropertyCollector();
            var nestedStates = contextState?.CloneNestedContext(ref nestedStateCollector) ?? Array.Empty<object>();
            var scopeProperties = contextState?.CaptureContextProperties(ref propertyCollector) ?? Array.Empty<KeyValuePair<string, object?>>();
            allProperties = new Dictionary<string, object?>(scopeProperties.Count, ScopeContext.DefaultComparer);
            ScopeContextPropertyEnumerator<object>.CopyScopePropertiesToDictionary(scopeProperties, allProperties);

            nestedContextTimestamp = 0L;
            if (nestedStates.Count > 0)
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

                nestedContext = nestedStates as object[] ?? nestedStates.ToArray();
            }
            else
            {
                nestedContext = Array.Empty<object>();
            }
        }

        object? IScopeContextAsyncState.NestedState => NestedContext?.Length > 0 ? NestedContext[0] : null;

        IList<object> IScopeContextAsyncState.CloneNestedContext(ref ScopeContextNestedStateCollector contextCollector)
        {
            if (contextCollector.IsCollectorEmpty)
            {
                return NestedContext?.Length > 0 ? NestedContext.ToArray() : Array.Empty<object>();   // We are done
            }
            else
            {
                for (int i = 0; i < NestedContext.Length; ++i)
                    contextCollector.CollectNestedState(NestedContext[i]);
                return contextCollector.StartCloneNestedContext(null); // We are done
            }
        }

        IReadOnlyCollection<KeyValuePair<string, object?>>? IScopeContextAsyncState.CaptureContextProperties(ref ScopeContextPropertyCollector contextCollector)
        {
            if (contextCollector.IsCollectorEmpty)
            {
                return MappedContext;   // We are done
            }
            else
            {
                return contextCollector.CaptureCompleted(MappedContext);     // We are done
            }
        }

        public override string ToString()
        {
            if (NestedContext?.Length > 0)
                return NestedContext[NestedContext.Length - 1]?.ToString() ?? "null";
            else
                return base.ToString() ?? GetType().ToString();
        }
    }
}

#endif
