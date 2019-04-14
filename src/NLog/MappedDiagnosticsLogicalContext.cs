// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !SILVERLIGHT

namespace NLog
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Internal;

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
        private sealed class ItemRemover : IDisposable
        {
            private readonly string _item1;
#if NET4_5
            // Optimized for HostingLogScope with 3 properties
            private readonly string _item2;
            private readonly string _item3;
            private readonly string[] _itemArray;
#endif
            //boolean as int to allow the use of Interlocked.Exchange
            private int _disposed;
            private readonly bool _wasEmpty;

            public ItemRemover(string item, bool wasEmpty)
            {
                _item1 = item;
                _wasEmpty = wasEmpty;
            }

#if NET4_5
            public ItemRemover(IReadOnlyList<KeyValuePair<string,object>> items, bool wasEmpty)
            {
                int itemCount = items.Count;
                if (itemCount > 2)
                {
                    _item1 = items[0].Key;
                    _item2 = items[1].Key;
                    _item3 = items[2].Key;
                    for (int i = 3; i < itemCount; ++i)
                    {
                        _itemArray = _itemArray ?? new string[itemCount - 3];
                        _itemArray[i - 3] = items[i].Key;
                    }
                }
                else if (itemCount > 1)
                {
                    _item1 = items[0].Key;
                    _item2 = items[1].Key;
                }
                else
                {
                    _item1 = items[0].Key;
                }
                _wasEmpty = wasEmpty;
            }
#endif

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 0)
                {
                    if (_wasEmpty && RemoveScopeWillClearContext())
                    {
                        Clear(true);
                        return;
                    }

                    var dictionary = GetLogicalThreadDictionary(true);
                    dictionary.Remove(_item1);
#if NET4_5
                    if (_item2 != null)
                    {
                        dictionary.Remove(_item2);
                        if (_item3 != null)
                            dictionary.Remove(_item3);
                        if (_itemArray != null)
                        {
                            for (int i = 0; i < _itemArray.Length; ++i)
                            {
                                if (_itemArray[i] != null)
                                    dictionary.Remove(_itemArray[i]);
                            }
                        }
                    }
#endif
                }
            }

            private bool RemoveScopeWillClearContext()
            {
#if NET4_5
                if (_itemArray == null)
                {
                    var immutableDict = GetLogicalThreadDictionary(false);
                    if ((immutableDict.Count == 1 && ReferenceEquals(_item2, null) && immutableDict.ContainsKey(_item1))
                      || (immutableDict.Count == 2 && !ReferenceEquals(_item2, null) && ReferenceEquals(_item3, null) && immutableDict.ContainsKey(_item1) && immutableDict.ContainsKey(_item2) && !_item1.Equals(_item2))
                      || (immutableDict.Count == 3 && !ReferenceEquals(_item3, null) && immutableDict.ContainsKey(_item1) && immutableDict.ContainsKey(_item2) && immutableDict.ContainsKey(_item3) && !_item1.Equals(_item2) && !_item1.Equals(_item3) && !_item2.Equals(_item3))
                      )
                    {
                        return true;
                    }
                }
#else
                var immutableDict = GetLogicalThreadDictionary(false);
                if (immutableDict.Count == 1 && immutableDict.ContainsKey(_item1))
                {
                    return true;
                }
#endif
                return false;
            }

            public override string ToString()
            {
                return _item1?.ToString() ?? base.ToString();
            }
        }

        /// <summary>
        /// Simulate ImmutableDictionary behavior (which is not yet part of all .NET frameworks).
        /// In future the real ImmutableDictionary could be used here to minimize memory usage and copying time.
        /// </summary>
        /// <param name="clone">Must be true for any subsequent dictionary modification operation</param>
        /// <param name="initialCapacity">Prepare dictionary for additional inserts</param>
        /// <returns></returns>
        private static IDictionary<string, object> GetLogicalThreadDictionary(bool clone = false, int initialCapacity = 0)
        {
            var dictionary = GetThreadLocal();
            if (dictionary == null)
            {
                if (!clone)
                    return EmptyDefaultDictionary;

                dictionary = new Dictionary<string, object>(initialCapacity);
                SetThreadLocal(dictionary);
            }
            else if (clone)
            {
                var newDictionary = new Dictionary<string, object>(dictionary.Count + initialCapacity);
                foreach (var keyValue in dictionary)
                    newDictionary[keyValue.Key] = keyValue.Value;
                SetThreadLocal(newDictionary);
                return newDictionary;
            }
            return dictionary;
        }

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
            if (!GetLogicalThreadDictionary().TryGetValue(item, out var value))
                return null;

#if NET4_6 || NETSTANDARD
            return value;
#else
            if (value is ObjectHandleSerializer objectHandle)
            {
                return objectHandle.Unwrap();
            }
            return value;
#endif
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
            var logicalContext = GetLogicalThreadDictionary(true, 1);
            bool wasEmpty = logicalContext.Count == 0;
            SetItemValue(item, value, logicalContext);
            return new ItemRemover(item, wasEmpty);
        }

#if NET4_5
        /// <summary>
        /// Updates the current logical context with multiple items in single operation
        /// </summary>
        /// <param name="items">.</param>
        /// <returns>>An <see cref="IDisposable"/> that can be used to remove the item from the current logical context (null if no items).</returns>
        public static IDisposable SetScoped(IReadOnlyList<KeyValuePair<string,object>> items)
        {
            if (items?.Count > 0)
            {
                var logicalContext = GetLogicalThreadDictionary(true, items.Count);
                bool wasEmpty = logicalContext.Count == 0;
                for (int i = 0; i < items.Count; ++i)
                {
                    SetItemValue(items[i].Key, items[i].Value, logicalContext);
                }
                return new ItemRemover(items, wasEmpty);
            }

            return null;
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
            var logicalContext = GetLogicalThreadDictionary(true, 1);
            SetItemValue(item, value, logicalContext);
        }

        private static void SetItemValue<T>(string item, T value, IDictionary<string, object> logicalContext)
        {
#if NET4_6 || NETSTANDARD
            logicalContext[item] = value;
#else
            if (typeof(T).IsValueType || Convert.GetTypeCode(value) != TypeCode.Object)
                logicalContext[item] = value;
            else
                logicalContext[item] = new ObjectHandleSerializer(value);
#endif
        }

        /// <summary>
        /// Returns all item names
        /// </summary>
        /// <returns>A collection of the names of all items in current logical context.</returns>
        public static ICollection<string> GetNames()
        {
            return GetLogicalThreadDictionary().Keys;
        }

        /// <summary>
        /// Checks whether the specified <paramref name="item"/> exists in current logical context.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>A boolean indicating whether the specified <paramref name="item"/> exists in current logical context.</returns>
        public static bool Contains(string item)
        {
            return GetLogicalThreadDictionary().ContainsKey(item);
        }

        /// <summary>
        /// Removes the specified <paramref name="item"/> from current logical context.
        /// </summary>
        /// <param name="item">Item name.</param>
        public static void Remove(string item)
        {
            GetLogicalThreadDictionary(true).Remove(item);
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
            if (free)
            {
                SetThreadLocal(null);
            }
            else
            {
                GetLogicalThreadDictionary(true).Clear();
            }
        }

        private static void SetThreadLocal(Dictionary<string, object> newValue)
        {
#if NET4_6 || NETSTANDARD
            AsyncLocalDictionary.Value = newValue;
#else
            if (newValue == null)
                System.Runtime.Remoting.Messaging.CallContext.FreeNamedDataSlot(LogicalThreadDictionaryKey);
            else
                System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(LogicalThreadDictionaryKey, newValue);
#endif
        }

        private static Dictionary<string, object> GetThreadLocal()
        {
#if NET4_6 || NETSTANDARD
            return AsyncLocalDictionary.Value;
#else
            return System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(LogicalThreadDictionaryKey) as Dictionary<string, object>;
#endif
        }

#if NET4_6 || NETSTANDARD
        private static readonly System.Threading.AsyncLocal<Dictionary<string, object>> AsyncLocalDictionary = new System.Threading.AsyncLocal<Dictionary<string, object>>();
#else
        private const string LogicalThreadDictionaryKey = "NLog.AsyncableMappedDiagnosticsContext";
#endif

        private static readonly IDictionary<string, object> EmptyDefaultDictionary = new SortHelpers.ReadOnlySingleBucketDictionary<string, object>();
    }
}

#endif

