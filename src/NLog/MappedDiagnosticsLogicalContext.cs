// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
        /// <summary>
        /// 
        /// </summary>
        private class ItemRemover : IDisposable
        {
            private readonly string _item;
            //boolean as int to allow the use of Interlocked.Exchange
            private int _disposed = 0;

            public ItemRemover(string item)
            {
                _item = item;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 0)
                {
                    Remove(_item); 
                }
            }
        }

        /// <summary>
        /// Simulate ImmutableDictionary behavior (which is not yet part of all .NET frameworks).
        /// In future the real ImmutableDictionary could be used here to minimize memory usage and copying time.
        /// </summary>
        /// <param name="clone">Must be true for any subsequent dictionary modification operation</param>
        /// <returns></returns>
        private static IDictionary<string, object> GetLogicalThreadDictionary(bool clone = false)
        {
            var dictionary = GetThreadLocal();
            if (dictionary == null)
            {
                if (!clone)
                    return EmptyDefaultDictionary;

                dictionary = new Dictionary<string, object>();
                SetThreadLocal(dictionary);
            }
            else if (clone)
            {
                dictionary = new Dictionary<string, object>(dictionary);
                SetThreadLocal(dictionary);
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
            object value;
            GetLogicalThreadDictionary().TryGetValue(item, out value);
            return value;
        }

        /// <summary>
        /// Sets the current logical context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        /// <returns>>An <see cref="IDisposable"/> that can be used to remove the item from the current logical context.</returns>
        public static IDisposable SetScoped(string item, string value)
        {
            Set(item, value);
            return new ItemRemover(item);
        }

        /// <summary>
        /// Sets the current logical context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        /// <returns>>An <see cref="IDisposable"/> that can be used to remove the item from the current logical context.</returns>
        public static IDisposable SetScoped(string item, object value)
        {
            Set(item, value);
            return new ItemRemover(item);
        }

        /// <summary>
        /// Sets the current logical context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        public static void Set(string item, string value)
        {
            GetLogicalThreadDictionary(true)[item] = value;
        }

        /// <summary>
        /// Sets the current logical context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        public static void Set(string item, object value)
        {
            GetLogicalThreadDictionary(true)[item] = value;
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
            Clear(false);
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

        private static void SetThreadLocal(IDictionary<string, object> newValue)
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

        private static IDictionary<string, object> GetThreadLocal()
        {
#if NET4_6 || NETSTANDARD
            return AsyncLocalDictionary.Value;
#else
            return System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(LogicalThreadDictionaryKey) as Dictionary<string, object>;
#endif
        }

#if NET4_6 || NETSTANDARD
        private static readonly System.Threading.AsyncLocal<IDictionary<string, object>> AsyncLocalDictionary = new System.Threading.AsyncLocal<IDictionary<string, object>>();
#else
        private const string LogicalThreadDictionaryKey = "NLog.AsyncableMappedDiagnosticsContext";
#endif

        private static readonly IDictionary<string, object> EmptyDefaultDictionary = new SortHelpers.ReadOnlySingleBucketDictionary<string, object>();
    }
}

#endif
