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

namespace NLog
{
    using System;
    using System.Collections.Generic;
    using NLog.Internal;

    /// <summary>
    /// Async version of Mapped Diagnostics Context - a logical context structure that keeps a dictionary
    /// of strings and provides methods to output them in layouts.  Allows for maintaining state across
    /// asynchronous tasks and call contexts.
    /// </summary>
    /// <remarks>
    /// Ideally, these changes should be incorporated as a new version of the MappedDiagnosticsContext class in the original
    /// NLog library so that state can be maintained for multiple threads in asynchronous situations.
    /// </remarks>
    public static class MappedDiagnosticsLogicalContext
    {
        /// <summary>
        /// Gets the current logical context named item, as <see cref="string"/>.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>The value of <paramref name="item"/>, if defined; otherwise <see cref="String.Empty"/>.</returns>
        /// <remarks>If the value isn't a <see cref="string"/> already, this call locks the <see cref="LogFactory"/> for reading the <see cref="Config.LoggingConfiguration.DefaultCultureInfo"/> needed for converting to <see cref="string"/>. </remarks>
        public static string Get(string item)
        {
            return Get(item, null);
        }

        /// <summary>
        /// Gets the current logical context named item, as <see cref="string"/>.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use when converting a value to a string.</param>
        /// <returns>The value of <paramref name="item"/>, if defined; otherwise <see cref="String.Empty"/>.</returns>
        /// <remarks>If <paramref name="formatProvider"/> is <c>null</c> and the value isn't a <see cref="string"/> already, this call locks the <see cref="LogFactory"/> for reading the <see cref="Config.LoggingConfiguration.DefaultCultureInfo"/> needed for converting to <see cref="string"/>. </remarks>
        public static string Get(string item, IFormatProvider formatProvider)
        {
            return FormatHelper.ConvertToString(GetObject(item), formatProvider);
        }

        /// <summary>
        /// Gets the current logical context named item, as <see cref="object"/>.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>The value of <paramref name="item"/>, if defined; otherwise <c>null</c>.</returns>
        public static object GetObject(string item)
        {
            if (ScopeContext.TryLookupProperty(item, out var value))
                return value;
            else
                return null;
        }

        /// <summary>
        /// Sets the current logical context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        /// <returns>>An <see cref="IDisposable"/> that can be used to remove the item from the current logical context.</returns>
        public static IDisposable SetScoped(string item, string value)
        {
            return SetScoped<string>(item, value);
        }

        /// <summary>
        /// Sets the current logical context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        /// <returns>>An <see cref="IDisposable"/> that can be used to remove the item from the current logical context.</returns>
        public static IDisposable SetScoped(string item, object value)
        {
            return SetScoped<object>(item, value);
        }

        /// <summary>
        /// Sets the current logical context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        /// <returns>>An <see cref="IDisposable"/> that can be used to remove the item from the current logical context.</returns>
        public static IDisposable SetScoped<T>(string item, T value)
        {
            return ScopeContext.PushProperty(item, value);
        }

#if !NET3_5 && !NET4_0
        /// <summary>
        /// Updates the current logical context with multiple items in single operation
        /// </summary>
        /// <param name="items">.</param>
        /// <returns>>An <see cref="IDisposable"/> that can be used to remove the item from the current logical context (null if no items).</returns>
        public static IDisposable SetScoped(IReadOnlyList<KeyValuePair<string, object>> items)
        {
            return ScopeContext.PushProperties(items);
        }
#endif
        /// <summary>
        /// Sets the current logical context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        public static void Set(string item, string value)
        {
            Set<string>(item, value);
        }

        /// <summary>
        /// Sets the current logical context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        public static void Set(string item, object value)
        {
            Set<object>(item, value);
        }

        /// <summary>
        /// Sets the current logical context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        public static void Set<T>(string item, T value)
        {
#if !NET35 && !NET40 && !NET45
            ScopeContext.SetMappedContextLegacy(item, value);
#else
            var oldContext = GetThreadLocal();
            var newContext = CloneDictionary(oldContext, 1);
            SetItemValue(item, value, newContext);
            SetThreadLocal(newContext);
#endif
        }

        /// <summary>
        /// Returns all item names
        /// </summary>
        /// <returns>A collection of the names of all items in current logical context.</returns>
        public static ICollection<string> GetNames()
        {
#if !NET35 && !NET40 && !NET45
            return new List<string>(System.Linq.Enumerable.Select(ScopeContext.GetAllProperties(), i => i.Key));
#else
            return GetThreadLocal()?.Keys ?? (ICollection<string>)ArrayHelper.Empty<string>();
#endif
        }

        /// <summary>
        /// Checks whether the specified <paramref name="item"/> exists in current logical context.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>A boolean indicating whether the specified <paramref name="item"/> exists in current logical context.</returns>
        public static bool Contains(string item)
        {
#if !NET35 && !NET40 && !NET45
            return ScopeContext.TryLookupProperty(item, out var _);
#else
            return GetThreadLocal()?.ContainsKey(item) == true;
#endif
        }

        /// <summary>
        /// Removes the specified <paramref name="item"/> from current logical context.
        /// </summary>
        /// <param name="item">Item name.</param>
        public static void Remove(string item)
        {
#if !NET35 && !NET40 && !NET45
            ScopeContext.RemoveMappedContextLegacy(item);
#else
            var oldContext = GetThreadLocal();
            if (oldContext?.ContainsKey(item)==true)
            {
                var newContext = CloneDictionary(oldContext, 0);
                newContext.Remove(item);
                SetThreadLocal(newContext);
            }
#endif
        }

        /// <summary>
        /// Clears the content of current logical context.
        /// </summary>
        public static void Clear()
        {
            Clear(true);
        }

        /// <summary>
        /// Clears the content of current logical context.
        /// </summary>
        /// <param name="free">Free the full slot.</param>
        public static void Clear(bool free)
        {
#if !NET35 && !NET40 && !NET45
            ScopeContext.ClearMappedContextLegacy();
#else
            ClearDictionary();
#endif
        }

#if NET35 || NET40 || NET45

#if !NET35 && !NET40
        internal static IDisposable PushProperties(IReadOnlyList<KeyValuePair<string, object>> properties)
        {
            if (properties?.Count > 0)
            {
                var oldContext = GetThreadLocal();
                var newContext = CloneDictionary(oldContext, properties.Count);
                for (int i = 0; i < properties.Count; ++i)
                    SetItemValue(properties[i].Key, properties[i].Value, newContext);
                SetThreadLocal(newContext);
                return new MappedScope(oldContext);
            }
            return null;
        }
#endif

        internal static IDisposable PushProperty<T>(string propertyName, T propertyValue)
        {
            var oldContext = GetThreadLocal();
            var newContext = CloneDictionary(oldContext, 1);
            SetItemValue(propertyName, propertyValue, newContext);
            SetThreadLocal(newContext);
            return new MappedScope(oldContext);
        }

        internal static bool TryLookupProperty(string propertyName, out object value)
        {
            var oldContext = GetThreadLocal();
            if (oldContext != null && oldContext.TryGetValue(propertyName, out value))
            {
                if (value is ObjectHandleSerializer objectHandle)
                    value = objectHandle.Unwrap();
                return true;
            }

            value = null;
            return false;
        }

        internal static void ClearDictionary()
        {
            SetThreadLocal(null);
        }

        internal static IEnumerable<KeyValuePair<string, object>> GetAllProperties()
        {
            var context = GetThreadLocal();
            if (context?.Count > 0)
            {
                foreach (var item in context)
                {
                    if (item.Value is ObjectHandleSerializer)
                    {
                        return GetAllPropertiesUnwrapped(context);
                    }
                }
                return context;
            }
            return ArrayHelper.Empty<KeyValuePair<string, object>>();
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

        private static Dictionary<string, object> CloneDictionary(Dictionary<string, object> oldContext, int initialCapacity = 0)
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

        private static void SetItemValue<T>(string item, T value, IDictionary<string, object> logicalContext)
        {
            object objectValue = value;
            if (Convert.GetTypeCode(objectValue) != TypeCode.Object)
                logicalContext[item] = objectValue;
            else
                logicalContext[item] = new ObjectHandleSerializer(objectValue);
        }

        private sealed class MappedScope : IDisposable
        {
            private readonly Dictionary<string, object> _oldContext;
            private bool _diposed;

            public MappedScope(Dictionary<string, object> oldContext)
            {
                _oldContext = oldContext;
            }

            public void Dispose()
            {
                if (!_diposed)
                {
                    SetThreadLocal(_oldContext);
                    _diposed = true;
                }
            }
        }

        private static void SetThreadLocal(Dictionary<string, object> newValue)
        {
            if (newValue == null)
                System.Runtime.Remoting.Messaging.CallContext.FreeNamedDataSlot(LogicalThreadDictionaryKey);
            else
                System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(LogicalThreadDictionaryKey, newValue);
        }

        private static Dictionary<string, object> GetThreadLocal()
        {
            return System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(LogicalThreadDictionaryKey) as Dictionary<string, object>;
        }

        private const string LogicalThreadDictionaryKey = "NLog.AsyncableMappedDiagnosticsContext";

        private static IEqualityComparer<string> DefaultComparer = StringComparer.OrdinalIgnoreCase;
#endif
    }
}