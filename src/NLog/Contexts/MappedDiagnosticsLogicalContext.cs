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

using System.Linq;

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
        /// <summary>
        /// Provides Dictionary-based Store for MDLC<br/>
        /// Store can be made readonly (by passing isReadOnly = true to constructor for class implementing ILexicon)<br/>
        /// Store is implemented as a .NET (Core) Dictionary in one of the following flavors:<br/>
        ///  - ClassicLexicon - based on Dictionary(string, object), Consecutive MDLC.SetScope(...) calls for same keys override values underneath<br/>
        ///  - StackedLexicon - based on Dictionary(string, Stack(object)), Consecutive MDLC.SetScope(...) calls for the same keys stack their values<br/>
        ///                     so calling MDLC.Dispose() cancels only the last MDLC.SetScope(...) effect and thus MDLC.Get() return previously set Value<br/>
        /// </summary>
        interface ILexicon
        {
            /// <summary>
            /// Returns keys inside the Store
            /// </summary>
            ICollection<string> Keys { get; }

            /// <summary>
            /// Get number of keys in the Store
            /// </summary>
            int Count { get; }

            /// <summary>
            /// Adds <paramref name="value"/> to the Store under given <paramref name="key"/><br/>
            /// Should throw NotSupportedException if is readonly
            /// </summary>
            /// <param name="key">Key for the Store</param>
            /// <param name="value">Value to be stored under <paramref name="key"/> in the Store</param>
            /// <param name="overrideValue">If true value for key is overriden in the Store (regardless of the Lexicon implementation)</param>
            void Add(string key, object value, bool overrideValue);

            /// <summary>
            /// Removes value from the Store under given <paramref name="key"/><br/>
            /// Removing a value that is not in the Store does nothing<br/>
            /// Should throw NotSupportedException if is readonly
            /// </summary>
            /// <param name="key">Key to be removed</param>
            void Remove(string key);

            /// <summary>
            /// Get current value for given key from the Store
            /// </summary>
            /// <param name="key">Key for the Store</param>
            /// <returns>Current value for the <paramref name="key"/> or null if key not found</returns>
            object Get(string key);

            /// <summary>
            /// Checks whether the Store contains entry for given key
            /// </summary>
            /// <param name="key">Key for lookup in the Store</param>
            /// <returns>Returns true if the Store contains given <paramref name="key"/>. False otherwise</returns>
            bool ContainsKey(string key);

            /// <summary>
            /// Tries to get the value associated with specified key and puts it into out value
            /// </summary>
            /// <param name="key">Key for lookup in the Store</param>
            /// <param name="value">If lookup is successful then value for given key is given by this out-param</param>
            /// <returns>Returns true if the function succeeds and key is found. False otherwise</returns>
            bool TryGetValue(string key, out object value);

            /// <summary>
            /// Clears the Store (removes all values from the Store)<br/>
            /// Should throw NotSupportedException if is readonly
            /// </summary>
            void Clear();

            /// <summary>
            /// Creates copy of the Lexicon with own instance of the Store<br/>
            /// This means deep copy of the keys and shallow/deep copy of the values (depends on the implementation)<br/>
            /// To be fully generic it is strongly recommended to implement value cloning as deep copy
            /// </summary>
            /// <param name="extraCapacity">Enlarge capacity for cloned Store by this value</param>
            /// <param name="madeCopyReadOnly">If true cloned Lexicon is made readonly</param>
            /// <returns>Independent copy of ILexicon</returns>
            ILexicon Clone(int extraCapacity = 0, bool madeCopyReadOnly = false);
        }

        /// <summary>
        /// ClassicLexicon - ILexicon based on Dictionary(string, object)<br/>
        /// When MDLC is using it then Consecutive MDLC.SetScope(...) calls for same keys override values underneath
        /// </summary>
        private sealed class ClassicLexicon : ILexicon
        {
            private bool _isReadOnly = false;
            private Dictionary<string, object> _store;

            /// <summary>
            /// Returns keys inside the Store
            /// </summary>
            ICollection<string> ILexicon.Keys => _store.Keys;

            /// <summary>
            /// Get number of keys in the Store
            /// </summary>
            int ILexicon.Count => _store.Count;

            /// <summary>
            /// Creates new ClassicLexicon
            /// Uses standard Dictionary(string, object) as internal store
            /// Not thread/async-safe
            /// </summary>
            public ClassicLexicon(bool isReadOnly = false, int initialCapacity = 0)
            {
                _isReadOnly = isReadOnly;
                _store = new Dictionary<string, object>(initialCapacity);
            }

            /// <summary>
            /// Creates new ClassicLexicon object using existing Dictionary (referenced, not copied)
            /// Uses standard Dictionary(string, object) as internal store
            /// Not thread/async-safe
            /// </summary>
            public ClassicLexicon(Dictionary<string, object> readyStore, bool isReadOnly = false)
            {
                _isReadOnly = isReadOnly;
                _store = readyStore;
            }

            /// <summary>
            /// Adds <paramref name="value"/> to the Store under given <paramref name="key"/><br/>
            /// Consecutive calls with the same <paramref name="key"/> overrides the value in the Store<br/>
            /// Throws NotSupportedException if is readonly
            /// </summary>
            /// <param name="key">Key for the Store</param>
            /// <param name="value">Value to be stored under <paramref name="key"/> in the Store</param>
            /// <param name="overrideValue">Parameter is ignored in this implementation</param>
            void ILexicon.Add(string key, object value, bool overrideValue)
            {
                if (_isReadOnly)
                    throw new NotSupportedException();

                _store[key] = value;
            }

            /// <summary>
            /// Removes value from the Store under given <paramref name="key"/><br/>
            /// Removing a value that is not in the Store does nothing<br/>
            /// Throws NotSupportedException if is readonly
            /// </summary>
            /// <param name="key">Key to be removed</param>
            void ILexicon.Remove(string key)
            {
                if (_isReadOnly)
                    throw new NotSupportedException();

                _store.Remove(key);
            }

            /// <summary>
            /// Get current value for given key from the Store
            /// </summary>
            /// <param name="key">Key for the Store</param>
            /// <returns>Current value for the <paramref name="key"/> or null if key not found</returns>
            object ILexicon.Get(string key)
            {
                _store.TryGetValue(key, out var value);
                return value;
            }

            /// <summary>
            /// Checks whether the Store contains entry for given key
            /// </summary>
            /// <param name="key">Key for lookup in the Store</param>
            /// <returns>Returns true if the Store contains given <paramref name="key"/>. False otherwise</returns>
            bool ILexicon.ContainsKey(string key)
            {
                return _store.ContainsKey(key);
            }

            /// <summary>
            /// Tries to get the value associated with specified key and puts it into out value
            /// </summary>
            /// <param name="key">Key for lookup in the Store</param>
            /// <param name="value">If lookup is successful then value for given key is given by this out-param</param>
            /// <returns>Returns true if the function succeeds and key is found. False otherwise</returns>
            bool ILexicon.TryGetValue(string key, out object value)
            {
                return _store.TryGetValue(key, out value);
            }

            /// <summary>
            /// Clears the Store (removes all values from the Store)<br/>
            /// Throws NotSupportedException if is readonly
            /// </summary>
            void ILexicon.Clear()
            {
                if (_isReadOnly)
                    throw new NotSupportedException();

                _store.Clear();
            }

            /// <summary>
            /// Creates copy of the Lexicon with own instance of the Store<br/><br/>
            /// This method uses shallow copy but for strings shallow copy works as deep copy<br/>
            /// So as long MDLC uses string as value type then it will work as intended
            /// </summary>
            /// <param name="extraCapacity">Enlarge capacity for cloned Store by this value</param>
            /// <param name="madeCopyReadOnly">If true cloned Lexicon is made readonly</param>
            /// <returns>Independent copy of ILexicon</returns>
            ILexicon ILexicon.Clone(int extraCapacity, bool madeCopyReadOnly)
            {
                var newStore = new Dictionary<string, object>(_store.Keys.Count + extraCapacity);
                foreach (var keyValue in _store)
                    newStore[keyValue.Key] = keyValue.Value; // shouldn't deep copy of the values be done here to be more generic?

                return new ClassicLexicon(newStore, madeCopyReadOnly);
            }
        }

        /// <summary>
        /// StackedLexicon - based on Dictionary(string, Stack(object)), Consecutive MDLC.SetScope(...) calls for the same keys stack their values,
        /// so calling MDLC.Dispose() cancels only the last MDLC.SetScope(...) effect and thus MDLC.Get() return previously set Value
        /// </summary>
        private sealed class StackedLexicon : ILexicon
        {
            private bool _isReadOnly = false;
            private Dictionary<string, Stack<object>> _store;

            /// <summary>
            /// Returns keys inside the Store
            /// </summary>
            ICollection<string> ILexicon.Keys => _store.Keys;

            /// <summary>
            /// Get number of keys in the Store
            /// </summary>
            int ILexicon.Count => _store.Count;

            /// <summary>
            /// Creates new StackedLexicon
            /// Uses Dictionary(string, Stack(object)) that enables Store to persist a history
            /// Not thread/async-safe
            /// </summary>
            public StackedLexicon(bool isReadOnly = false, int initialCapacity = 0)
            {
                _isReadOnly = isReadOnly;
                _store = new Dictionary<string, Stack<object>>(initialCapacity);
            }

            /// <summary>
            /// Creates new StackedLexicon with existing Dictionary (referenced, not copied)
            /// Uses Dictionary(string, Stack(object)) that enables Store to persist a history
            /// Not thread/async-safe
            /// </summary>
            public StackedLexicon(Dictionary<string, Stack<object>> readyStore, bool isReadOnly = false)
            {
                _isReadOnly = isReadOnly;
                _store = readyStore;
            }

            /// <summary>
            /// Adds <paramref name="value"/> to the Store under given <paramref name="key"/><br/>
            /// If stack exists under given <paramref name="key"/> then <paramref name="value"/> is added to it<br/>
            /// Otherwise stack with one element (<paramref name="value"/>) is created<br/>
            /// So consecutive calls with the same <paramref name="key"/> stack values in the Store creating history<br/>
            /// Throws NotSupportedException if is readonly
            /// </summary>
            /// <param name="key">Key for the Store</param>
            /// <param name="value">Value to be stored under <paramref name="key"/> in the Store</param>
            /// <param name="overrideValue">If true value for key is overriden in the Store (instead of being stacked)</param>
            void ILexicon.Add(string key, object value, bool overrideValue)
            {
                _store.TryGetValue(key, out var stack);

                if (stack == null || overrideValue)
                    stack = new Stack<object>();

                stack.Push(value);

                _store[key] = stack;
            }

            /// <summary>
            /// Removes value from the Store under given <paramref name="key"/><br/>
            /// If stack exists under given <paramref name="key"/> then top element is removed (popped)<br/>
            /// Otherwise nothing is done
            /// Consequently, removing a value that is not in the Store does nothing<br/>
            /// Throws NotSupportedException if is readonly
            /// </summary>
            /// <param name="key">Key to be removed</param>
            void ILexicon.Remove(string key)
            {
                if (!_store.TryGetValue(key, out var stack))
                    return;

                // if stack.Count == 0 then remove? // possible sanity check

                if (stack != null && stack.Count > 0)
                {
                    stack.Pop();

                    // maintain compatibility with existing MDLC - no value then no key in the dictionary
                    if (stack.Count == 0)
                        _store.Remove(key);
                }
            }

            /// <summary>
            /// Get current value for given <paramref name="key"/> from the Store<br/>
            /// In case of multiple values (on the stack) it returns the one from the top of the stack (by peeking - without popping it)<br/>
            /// </summary>
            /// <param name="key">Key for the Store</param>
            /// <returns>Current (last) value for the <paramref name="key"/> or null if key not found</returns>
            object ILexicon.Get(string key)
            {
                return _store.TryGetValue(key, out var stack)
                    ? stack != null && stack.Count > 0
                        ? stack.Peek()
                        : null
                    : null;
            }

            /// <summary>
            /// Checks whether the Store contains entry for given key
            /// </summary>
            /// <param name="key">Key for lookup in the Store</param>
            /// <returns>Returns true if the Store contains given <paramref name="key"/>. False otherwise</returns>
            bool ILexicon.ContainsKey(string key)
            {
                return _store.ContainsKey(key); // && value not null - not necessary since stack is always allocated
            }

            /// <summary>
            /// Tries to get the value associated with specified key and puts it into out value
            /// </summary>
            /// <param name="key">Key for lookup in the Store</param>
            /// <param name="value">If lookup is successful then value for given key is given by this out-param</param>
            /// <returns>Returns true if the function succeeds and key is found. False otherwise</returns>
            bool ILexicon.TryGetValue(string key, out object value)
            {
                var result = _store.TryGetValue(key, out var stack);
                if (stack != null && stack.Count > 0)
                    value = stack.Peek();
                else
                    value = null;
                return result;
            }

            /// <summary>
            /// Clears the Store (removes all values from the Store)<br/>
            /// Throws NotSupportedException if is readonly
            /// </summary>
            void ILexicon.Clear()
            {
                _store.Clear();
            }

            /// <summary>
            /// Creates copy of the Lexicon with own instance of the Store<br/><br/>
            /// This method uses deep copy of the stack and shallow copy of the strings which works as deep copy<br/>
            /// So as long MDLC uses string as stored values type then it will work as intended
            /// </summary>
            /// <param name="extraCapacity">Enlarge capacity for cloned Store by this value</param>
            /// <param name="madeCopyReadOnly">If true cloned Lexicon is made readonly</param>
            /// <returns>Independent copy of ILexicon</returns>
            ILexicon ILexicon.Clone(int extraCapacity, bool madeCopyReadOnly)
            {
                var newStore = new Dictionary<string, Stack<object>>(_store.Keys.Count + extraCapacity);
                foreach (var keyValue in _store)
                    newStore[keyValue.Key] = new Stack<object>(keyValue.Value.Reverse());

                return new StackedLexicon(newStore, madeCopyReadOnly);
            }
        }

        private static volatile bool _useStackedStore;

        /// <summary>
        /// If true then internal Store is based on Dictionary(string, Stack(object)) => Consecutive MDLC.SetScope(...) calls for the same keys stack their values,
        /// so calling MDLC.Dispose() cancels only the last MDLC.SetScope(...) effect and thus MDLC.Get() return previously set Value.
        /// If false then internal Store is based on Dictionary(string, object) => Consecutive MDLC.SetScope(...) calls for same keys override values
        /// </summary>
        public static bool UseStackedStore
        {
            get => _useStackedStore;

            set
            {
                if (GetThreadLocal() != null)
                    throw new InvalidOperationException("Changing MDLC Store mode is not permitted after it is used");

                _useStackedStore = value;
            }
        }

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
        private static ILexicon GetLogicalThreadDictionary(bool clone = false, int initialCapacity = 0)
        {
            var dictionary = GetThreadLocal();
            if (dictionary == null)
            {
                if (!clone)
                    return GetNewReadOnlyStore;

                dictionary = GetNewStore;
                SetThreadLocal(dictionary);
            }
            else if (clone)
            {
                var newDictionary = dictionary.Clone(extraCapacity: initialCapacity);

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
            SetItemValue(item, value, logicalContext, overrideValue: false);
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
                    SetItemValue(items[i].Key, items[i].Value, logicalContext, overrideValue: false);
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
            SetItemValue(item, value, logicalContext, overrideValue: true);
        }

        private static void SetItemValue<T>(string item, T value, ILexicon logicalContext, bool overrideValue)
        {
#if NET4_6 || NETSTANDARD
            logicalContext.Add(item, value, overrideValue);
#else
            if (typeof(T).IsValueType || Convert.GetTypeCode(value) != TypeCode.Object)
                logicalContext.Add(item, value, overrideValue);
            else
                logicalContext.Add(item, new ObjectHandleSerializer(value), overrideValue);
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

        private static void SetThreadLocal(ILexicon newDict)
        {
#if NET4_6 || NETSTANDARD
            AsyncLocalDictionary.Value = newDict;
#else
            if (newDict == null)
                System.Runtime.Remoting.Messaging.CallContext.FreeNamedDataSlot(LogicalThreadDictionaryKey);
            else
                System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(LogicalThreadDictionaryKey, newDict);
#endif
        }

        private static ILexicon GetThreadLocal()
        {
#if NET4_6 || NETSTANDARD
            return AsyncLocalDictionary.Value;
#else
            return System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(LogicalThreadDictionaryKey) as ILexicon;
#endif
        }

#if NET4_6 || NETSTANDARD
        private static System.Threading.AsyncLocal<ILexicon> AsyncLocalDictionary { get; } = new System.Threading.AsyncLocal<ILexicon>();
#else
        private const string LogicalThreadDictionaryKey = "NLog.AsyncableMappedDiagnosticsContext";
#endif

        private static ILexicon GetNewStore
        {
            get
            {
                if (UseStackedStore)
                    return new StackedLexicon(false);
                else
                    return new ClassicLexicon(false);
            }
        }

        private static ILexicon GetNewReadOnlyStore
        {
            get
            {
                if (UseStackedStore)
                    return new StackedLexicon(true);
                else
                    return new ClassicLexicon(true);
            }
        }
    }
}

#endif