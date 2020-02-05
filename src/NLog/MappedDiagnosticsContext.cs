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

namespace NLog
{
    using System;
    using System.Collections.Generic;

    using Config;
    using Internal;

    /// <summary>
    /// Mapped Diagnostics Context - a thread-local structure that keeps a dictionary
    /// of strings and provides methods to output them in layouts. 
    /// </summary>
    public static class MappedDiagnosticsContext
    {
        private static readonly object DataSlot = ThreadLocalStorageHelper.AllocateDataSlot();

        private static readonly IDictionary<string, object> EmptyDefaultDictionary = new SortHelpers.ReadOnlySingleBucketDictionary<string, object>();

        private sealed class ItemRemover : IDisposable
        {
            private readonly string _item;
            private bool _disposed;

            public ItemRemover(string item)
            {
                _item = item;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    Remove(_item);
                }
            }
        }

        /// <summary>
        /// Gets the thread-local dictionary
        /// </summary>
        /// <param name="create">Must be true for any subsequent dictionary modification operation</param>
        /// <returns></returns>
        private static IDictionary<string, object> GetThreadDictionary(bool create = true)
        {
            var dictionary = ThreadLocalStorageHelper.GetDataForSlot<Dictionary<string, object>>(DataSlot, create);
            if (dictionary == null && !create)
                return EmptyDefaultDictionary;

            return dictionary;
        }

        /// <summary>
        /// Sets the current thread MDC item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to remove the item from the current thread MDC.</returns>
        public static IDisposable SetScoped(string item, string value)
        {
            Set(item, value);
            return new ItemRemover(item);
        }

        /// <summary>
        /// Sets the current thread MDC item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        /// <returns>>An <see cref="IDisposable"/> that can be used to remove the item from the current thread MDC.</returns>
        public static IDisposable SetScoped(string item, object value)
        {
            Set(item, value);
            return new ItemRemover(item);
        }

        /// <summary>
        /// Sets the current thread MDC item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        public static void Set(string item, string value)
        {
            GetThreadDictionary(true)[item] = value;
        }

        /// <summary>
        /// Sets the current thread MDC item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        public static void Set(string item, object value)
        {
            GetThreadDictionary(true)[item] = value;
        }

        /// <summary>
        /// Gets the current thread MDC named item, as <see cref="string"/>.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>The value of <paramref name="item"/>, if defined; otherwise <see cref="String.Empty"/>.</returns>
        /// <remarks>If the value isn't a <see cref="string"/> already, this call locks the <see cref="LogFactory"/> for reading the <see cref="LoggingConfiguration.DefaultCultureInfo"/> needed for converting to <see cref="string"/>. </remarks>
        public static string Get(string item)
        {
            return Get(item, null);
        }

        /// <summary>
        /// Gets the current thread MDC named item, as <see cref="string"/>.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use when converting a value to a <see cref="string"/>.</param>
        /// <returns>The value of <paramref name="item"/>, if defined; otherwise <see cref="String.Empty"/>.</returns>
        /// <remarks>If <paramref name="formatProvider"/> is <c>null</c> and the value isn't a <see cref="string"/> already, this call locks the <see cref="LogFactory"/> for reading the <see cref="LoggingConfiguration.DefaultCultureInfo"/> needed for converting to <see cref="string"/>. </remarks>
        public static string Get(string item, IFormatProvider formatProvider)
        {
            return FormatHelper.ConvertToString(GetObject(item), formatProvider);
        }

        /// <summary>
        /// Gets the current thread MDC named item, as <see cref="object"/>.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>The value of <paramref name="item"/>, if defined; otherwise <c>null</c>.</returns>
        public static object GetObject(string item)
        {
            object o;

            if (!GetThreadDictionary(false).TryGetValue(item, out o))
                o = null;

            return o;
        }

        /// <summary>
        /// Returns all item names
        /// </summary>
        /// <returns>A set of the names of all items in current thread-MDC.</returns>
        public static ICollection<string> GetNames()
        {
            return GetThreadDictionary(false).Keys;
        }

        /// <summary>
        /// Checks whether the specified item exists in current thread MDC.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>A boolean indicating whether the specified <paramref name="item"/> exists in current thread MDC.</returns>
        public static bool Contains(string item)
        {
            return GetThreadDictionary(false).ContainsKey(item);
        }

        /// <summary>
        /// Removes the specified <paramref name="item"/> from current thread MDC.
        /// </summary>
        /// <param name="item">Item name.</param>
        public static void Remove(string item)
        {
            GetThreadDictionary(true).Remove(item);
        }

        /// <summary>
        /// Clears the content of current thread MDC.
        /// </summary>
        public static void Clear()
        {
            GetThreadDictionary(true).Clear();
        }
    }
}