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
    using System.Threading;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Property collectors are sub-contexts of the NLog.ScopeContext.
    /// They are mutable, async-safe and thread-safe.
    /// They should be used when there is a need to set context values from child async contexts (such as an async method call) which should be used in logs created by the parent context.
    /// </summary>
    internal class ScopeContextAsyncPropertyCollector : IDisposable
    {
        /// <summary>
        /// The actual (async-local) stack of contexts.
        /// </summary>
        private static readonly AsyncLocal<ScopeContextAsyncPropertyCollector> PropertyCollectorContext = new AsyncLocal<ScopeContextAsyncPropertyCollector>();

        // Note: cannot use ScopeContextPropertyEnumerator which is a read-only dictionary-like object - the specificity of this context is to be mutable.
        /// <summary>
        /// The values stored inside the context.
        /// </summary>
        private ConcurrentDictionary<string, object> _values;

        /// <summary>
        /// Parent context. Null for the first context in the chain. Useful since we need to be able to dispose the context without changing async context.
        /// </summary>
        private ScopeContextAsyncPropertyCollector _parent = null;

        #region Construction & copy
        private ScopeContextAsyncPropertyCollector()
        {
            _values = new ConcurrentDictionary<string, object>();
        }

        private ScopeContextAsyncPropertyCollector(ScopeContextAsyncPropertyCollector parent)
        {
            _parent = parent;
            // Always copy to avoid side effects.            
            _values = new ConcurrentDictionary<string, object>(parent._values);
        }
        #endregion Construction & copy

        #region IDisposable
        private bool _disposed;

        void IDisposable.Dispose()
        {
            if (!_disposed)
            {
                PropertyCollectorContext.Value = _parent;
                _parent = null;
                _disposed = true;
            }
        }
        #endregion IDisposable

        #region Stack handling
        /// <summary>
        /// Create a new property collector context on top of the async-local stack.
        /// </summary>
        internal static ScopeContextAsyncPropertyCollector PushSnapshot()
        {
            ScopeContextAsyncPropertyCollector newScope;
            if (PropertyCollectorContext.Value != null)
            {
                newScope = new ScopeContextAsyncPropertyCollector(PropertyCollectorContext.Value);
            }
            else
            {
                newScope = new ScopeContextAsyncPropertyCollector();
            }
            PropertyCollectorContext.Value = newScope;
            return newScope;
        }

        private static ScopeContextAsyncPropertyCollector GetAsyncLocalCollector()
        {
            return PropertyCollectorContext.Value ?? PushSnapshot();
        }
        #endregion Stack handling


        /// <summary>
        /// Add a property to the current top-level collector inside the collector stack.
        /// A collector is created if the stack is empty.
        /// If the key already exists inside the collector, the value is replaced by the new one.
        /// </summary>
        internal static void CollectProperty(string key, object value)
        {
            GetAsyncLocalCollector()._values[key] = value;
        }

        /// <summary>
        /// Removes a property from the current top-level collector inside the collector stack.
        /// No error if the key does not exist inside the collector.
        /// </summary>
        internal static void RemoveProperty(string key)
        {
            // Do not create a collector if stack is empty!
            if (PropertyCollectorContext.Value == null)
            {
                return;
            }
            PropertyCollectorContext.Value._values.TryRemove(key, out _);
        }

        /// <summary>
        /// Fully clear the current collector.
        /// </summary>
        internal static void Clear()
        {
            PropertyCollectorContext.Value = null;
        }

        internal static bool TryGetProperty(string key, out object value)
        {
            if (PropertyCollectorContext.Value == null)
            {
                value = null;
                return false;
            }
            return PropertyCollectorContext.Value._values.TryGetValue(key, out value);
        }

        internal static IEnumerable<KeyValuePair<string, object>> GetAllProperties()
        {
            return PropertyCollectorContext.Value == null ? (IEnumerable<KeyValuePair<string, object>>)ArrayHelper.Empty<KeyValuePair<string, object>>() : GetAsyncLocalCollector()._values;
        }
    }
}

#endif