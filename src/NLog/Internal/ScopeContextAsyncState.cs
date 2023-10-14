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
        IReadOnlyCollection<KeyValuePair<string, object>> CaptureContextProperties(ref ScopeContextPropertyCollector contextCollector);
        IList<object> CaptureNestedContext(ref ScopeContextNestedStateCollector contextCollector);
    }

    struct ScopeContextPropertyCollector
    {
        IReadOnlyCollection<KeyValuePair<string, object>> _allProperties;
        List<KeyValuePair<string, object>> _propertyCollector;

        public bool IsCollectorEmpty => _allProperties is null || (_allProperties.Count == 0 && _propertyCollector is null);

        public bool IsCollectorInactive => _allProperties is null;

        public ScopeContextPropertyCollector(List<KeyValuePair<string, object>> propertyCollector = null)
        {
            _allProperties = _propertyCollector = propertyCollector;
        }

        public IReadOnlyCollection<KeyValuePair<string, object>> StartCaptureProperties(IScopeContextAsyncState state)
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

        public IReadOnlyCollection<KeyValuePair<string, object>> CaptureCompleted(IReadOnlyCollection<KeyValuePair<string, object>> properties)
        {
            if (_allProperties?.Count > 0)
            {
                if (properties?.Count > 0)
                {
                    if (_propertyCollector is null)
                    {
                        return _allProperties = MergeUniqueProperties(properties);
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
                    return _allProperties = Array.Empty<KeyValuePair<string, object>>();
            }
        }

        private IReadOnlyCollection<KeyValuePair<string, object>> MergeUniqueProperties(IReadOnlyCollection<KeyValuePair<string, object>> properties)
        {
            var scopeProperties = new Dictionary<string, object>(_allProperties.Count + properties.Count, ScopeContext.DefaultComparer);
            ScopeContextPropertyEnumerator<object>.CopyScopePropertiesToDictionary(properties, scopeProperties);
            ScopeContextPropertyEnumerator<object>.CopyScopePropertiesToDictionary(_allProperties, scopeProperties);
            return scopeProperties;
        }

        private static IReadOnlyCollection<KeyValuePair<string, object>> EnsureUniqueProperties(IReadOnlyCollection<KeyValuePair<string, object>> properties)
        {
            if (properties.Count > 1)
            {
                // Must validate that collected properties are unique
                if (properties is Dictionary<string, object> dictionary && ReferenceEquals(dictionary.Comparer, ScopeContext.DefaultComparer))
                {
                    return properties;
                }
                else if (properties.Count > 10 || !ScopeContextPropertyEnumerator<object>.HasUniqueCollectionKeys(properties, ScopeContext.DefaultComparer))
                {
                    var scopeProperties = new Dictionary<string, object>(Math.Min(properties.Count, 10), ScopeContext.DefaultComparer);
                    ScopeContextPropertyEnumerator<object>.CopyScopePropertiesToDictionary(properties, scopeProperties);
                    return scopeProperties;
                }
            }

            return properties;
        }

        public void AddProperty(string propertyName, object propertyValue)
        {
            if (IsCollectorEmpty)
            {
                _allProperties = new[] { new KeyValuePair<string, object>(propertyName, propertyValue) };
            }
            else
            {
                if (_propertyCollector is null)
                {
                    _propertyCollector = new List<KeyValuePair<string, object>>(Math.Max(4, _allProperties.Count + 1));
                    _propertyCollector.Add(new KeyValuePair<string, object>(propertyName, propertyValue));
                    CollectProperties(_allProperties);
                    _allProperties = _propertyCollector;
                }
                else
                {
                    _propertyCollector.Insert(0, new KeyValuePair<string, object>(propertyName, propertyValue));
                }
            }
        }

        public void AddProperties(IReadOnlyCollection<KeyValuePair<string, object>> properties)
        {
            if (IsCollectorEmpty)
            {
                _allProperties = properties;
            }
            else if (properties?.Count > 0)
            {
                if (_propertyCollector is null)
                {
                    _propertyCollector = new List<KeyValuePair<string, object>>(Math.Max(4, _allProperties.Count + properties.Count));
                    CollectProperties(properties);
                    CollectProperties(_allProperties);
                    _allProperties = _propertyCollector;
                }
                else if (_propertyCollector.Count == 0)
                {
                    CollectProperties(properties);
                }
                else
                {
                    int insertPosition = 0;
                    using (var scopeEnumerator = new ScopeContextPropertyEnumerator<object>(properties))
                    {
                        while (scopeEnumerator.MoveNext())
                        {
                            var property = scopeEnumerator.Current;
                            _propertyCollector.Insert(insertPosition++, property);
                        }
                    }
                }
            }
        }

        private void CollectProperties(IReadOnlyCollection<KeyValuePair<string, object>> properties)
        {
            using (var scopeEnumerator = new ScopeContextPropertyEnumerator<object>(properties))
            {
                while (scopeEnumerator.MoveNext())
                {
                    var property = scopeEnumerator.Current;
                    _propertyCollector.Add(property);
                }
            }
        }
    }

    struct ScopeContextNestedStateCollector
    {
        private IList<object> _allNestedStates;
        private List<object> _nestedStateCollector;

        public bool IsCollectorEmpty => _allNestedStates is null || (_allNestedStates.Count == 0 && _nestedStateCollector is null);

        public bool IsCollectorInactive => _allNestedStates is null;

        public IList<object> StartCaptureNestedStates(IScopeContextAsyncState state)
        {
            _allNestedStates = _allNestedStates ?? Array.Empty<object>();

            while (state != null)
            {
                var result = state.CaptureNestedContext(ref this);
                if (result != null)
                    return result;
                state = state.Parent;
            }

            return _allNestedStates;
        }

        public void PushNestedState(object state)
        {
            if (_nestedStateCollector is null)
            {
                _nestedStateCollector = new List<object>(Math.Max(4, _allNestedStates?.Count ?? 0 + 1));
                if (_allNestedStates?.Count > 0)
                {
                    for (int i = 0; i < _allNestedStates.Count; ++i)
                        _nestedStateCollector.Add(_allNestedStates[i]);
                }
                _allNestedStates = _nestedStateCollector;
            }
            _nestedStateCollector.Add(state);    // Collected in "reversed" order
        }
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

        IList<object> IScopeContextAsyncState.CaptureNestedContext(ref ScopeContextNestedStateCollector contextCollector)
        {
            if (contextCollector.IsCollectorEmpty)
            {
                if (Parent is null)
                {
                    return new object[] { _value }; // We are done
                }
                else if (contextCollector.IsCollectorInactive)
                {
                    contextCollector.PushNestedState(_value);   // Mark as active
                    return contextCollector.StartCaptureNestedStates(Parent);
                }
            }

            contextCollector.PushNestedState(_value);
            return null;    // continue with parent
        }

        IReadOnlyCollection<KeyValuePair<string, object>> IScopeContextAsyncState.CaptureContextProperties(ref ScopeContextPropertyCollector contextCollector)
        {
            if (contextCollector.IsCollectorInactive)
            {
                if (Parent is null)
                {
                    return Array.Empty<KeyValuePair<string, object>>(); // We are done
                }
                else
                {
                    contextCollector.AddProperties(Array.Empty<KeyValuePair<string, object>>());    // Mark as active
                    return contextCollector.StartCaptureProperties(Parent);   // Start parent enumeration
                }
            }
            else
            {
                return null;    // Continue with Parent
            }
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

        IList<object> IScopeContextAsyncState.CaptureNestedContext(ref ScopeContextNestedStateCollector contextCollector)
        {
            if (contextCollector.IsCollectorInactive)
            {
                if (Parent is null)
                {
                    return Array.Empty<object>();   // We are done
                }
                else
                {
                    return contextCollector.StartCaptureNestedStates(Parent);
                }
            }

            return null;    // Continue with Parent
        }

        IReadOnlyCollection<KeyValuePair<string, object>> IScopeContextAsyncState.CaptureContextProperties(ref ScopeContextPropertyCollector contextCollector)
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
            else
            {
                if (_allProperties is null)
                {
                    contextCollector.AddProperty(Name, Value);
                    return null;    // Continue with Parent
                }
                else
                {
                    return contextCollector.CaptureCompleted(_allProperties);     // We are done
                }
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
            : base(parent)
        {
            _allProperties = allProperties; // Collapsed dictionary that includes all properties from parent scopes with case-insensitive-comparer
        }

        public ScopeContextPropertiesAsyncState(IScopeContextAsyncState parent, Dictionary<string, object> allProperties, object nestedState)
            : base(parent)
        {
            _allProperties = allProperties; // Collapsed dictionary that includes all properties from parent scopes with case-insensitive-comparer
            NestedState = nestedState;
            NestedStateTimestamp = ScopeContext.GetNestedContextTimestampNow();
        }

        public ScopeContextPropertiesAsyncState(IScopeContextAsyncState parent, IReadOnlyCollection<KeyValuePair<string, TValue>> scopeProperties)
            : base(parent)
        {
            _scopeProperties = scopeProperties;
        }

        public ScopeContextPropertiesAsyncState(IScopeContextAsyncState parent, IReadOnlyCollection<KeyValuePair<string, TValue>> scopeProperties, object nestedState)
            : base(parent)
        {
            _scopeProperties = scopeProperties;
            NestedState = nestedState;
            NestedStateTimestamp = ScopeContext.GetNestedContextTimestampNow();
        }

        IList<object> IScopeContextAsyncState.CaptureNestedContext(ref ScopeContextNestedStateCollector contextCollector)
        {
            if (NestedState is null)
            {
                if (contextCollector.IsCollectorInactive)
                    return contextCollector.StartCaptureNestedStates(Parent);
                else
                    return null;    // continue with parent
            }
            else
            {
                if (contextCollector.IsCollectorEmpty)
                {
                    if (Parent is null)
                    {
                        return new object[] { NestedState };    // We are done
                    }
                    else if (contextCollector.IsCollectorInactive)
                    {
                        contextCollector.PushNestedState(NestedState);  // Mark as active
                        return contextCollector.StartCaptureNestedStates(Parent);
                    }
                }

                contextCollector.PushNestedState(NestedState);
                return null;    // continue with parent
            }
        }

        IReadOnlyCollection<KeyValuePair<string, object>> IScopeContextAsyncState.CaptureContextProperties(ref ScopeContextPropertyCollector contextCollector)
        {
            if (contextCollector.IsCollectorEmpty)
            {
                if (_allProperties is null)
                {
                    contextCollector.AddProperties(_scopeProperties as IReadOnlyCollection<KeyValuePair<string, object>> ?? this);
                    _allProperties = contextCollector.StartCaptureProperties(Parent); // Capture all properties from parents
                }
                return _allProperties;  // We are done
            }
            else
            {
                if (_allProperties is null)
                {
                    contextCollector.AddProperties(_scopeProperties as IReadOnlyCollection<KeyValuePair<string, object>> ?? this);
                    return null;    // Continue with Parent
                }
                else
                {
                    return contextCollector.CaptureCompleted(_allProperties);     // We are done
                }
            }
        }

        public override string ToString()
        {
            return NestedState?.ToString() ?? $"Count = {Count}";
        }

        public int Count => _scopeProperties.Count;

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => new ScopeContextPropertyEnumerator<TValue>(_scopeProperties);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new ScopeContextPropertyEnumerator<TValue>(_scopeProperties);
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
            var nestedStateCollector = new ScopeContextNestedStateCollector();
            var propertyCollector = new ScopeContextPropertyCollector();
            var nestedStates = contextState?.CaptureNestedContext(ref nestedStateCollector) ?? Array.Empty<object>();
            var scopeProperties = contextState?.CaptureContextProperties(ref propertyCollector) ?? Array.Empty<KeyValuePair<string, object>>();
            allProperties = new Dictionary<string, object>(scopeProperties.Count, ScopeContext.DefaultComparer);
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

                nestedContext = nestedStates as object[];
                if (nestedContext == null)
                    nestedContext = System.Linq.Enumerable.ToArray(nestedStates);
            }
            else
            {
                nestedContext = Array.Empty<object>();
            }
        }

        object IScopeContextAsyncState.NestedState => NestedContext?.Length > 0 ? NestedContext[0] : null;

        IList<object> IScopeContextAsyncState.CaptureNestedContext(ref ScopeContextNestedStateCollector contextCollector)
        {
            if (contextCollector.IsCollectorEmpty)
            {
                if (NestedContext?.Length > 0)
                {
                    var nestedStates = new object[NestedContext.Length];
                    for (int i = 0; i < nestedStates.Length; ++i)
                        nestedStates[i] = NestedContext[i];
                    return nestedStates;            // We are done
                }
                else
                {
                    return Array.Empty<object>();   // We are done
                }
            }
            else
            {
                for (int i = 0; i < NestedContext.Length; ++i)
                    contextCollector.PushNestedState(NestedContext[i]);
                return contextCollector.StartCaptureNestedStates(null); // We are done
            }
        }

        IReadOnlyCollection<KeyValuePair<string, object>> IScopeContextAsyncState.CaptureContextProperties(ref ScopeContextPropertyCollector contextCollector)
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
                return base.ToString();
        }
    }
}

#endif