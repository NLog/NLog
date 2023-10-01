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

namespace NLog
{
    using System;
    using System.Collections.Generic;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Global Diagnostics Context - a dictionary structure to hold per-application-instance values.
    /// </summary>
    public static class GlobalDiagnosticsContext
    {
        private static readonly object _lockObject = new object();
        private static Dictionary<string, object> _dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, object> _dictReadOnly;  // Reset cache on change

        /// <summary>
        /// Sets the value with the specified key in the Global Diagnostics Context (GDC) dictionary
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        public static void Set(string item, string value)
        {
            Set(item, (object)value);
        }

        /// <summary>
        /// Sets the value with the specified key in the Global Diagnostics Context (GDC) dictionary
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        public static void Set(string item, object value)
        {
            lock (_lockObject)
            {
                GetWritableDict()[item] = value;
            }
        }

        /// <summary>
        /// Gets the value with the specified key in the Global Diagnostics Context (GDC) dictionary
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>The value of <paramref name="item"/>, if defined; otherwise <see cref="String.Empty"/>.</returns>
        /// <remarks>If the value isn't a <see cref="string"/> already, this call locks the <see cref="LogFactory"/> for reading the <see cref="LoggingConfiguration.DefaultCultureInfo"/> needed for converting to <see cref="string"/>. </remarks>
        public static string Get(string item)
        {
            return Get(item, null);
        }

        /// <summary>
        /// Gets the value with the specified key in the Global Diagnostics Context (GDC) dictionary
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> to use when converting the item's value to a string.</param>
        /// <returns>The value of <paramref name="item"/> as a string, if defined; otherwise <see cref="String.Empty"/>.</returns>
        /// <remarks>If <paramref name="formatProvider"/> is <c>null</c> and the value isn't a <see cref="string"/> already, this call locks the <see cref="LogFactory"/> for reading the <see cref="LoggingConfiguration.DefaultCultureInfo"/> needed for converting to <see cref="string"/>. </remarks>
        public static string Get(string item, IFormatProvider formatProvider)
        {
            return FormatHelper.ConvertToString(GetObject(item), formatProvider);
        }

        /// <summary>
        /// Gets the value with the specified key in the Global Diagnostics Context (GDC) dictionary
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>The item value, if defined; otherwise <c>null</c>.</returns>
        public static object GetObject(string item)
        {
            GetReadOnlyDict().TryGetValue(item, out var o);
            return o;
        }

        /// <summary>
        /// Gets all key-names from Global Diagnostics Context (GDC) dictionary
        /// </summary>
        /// <returns>A collection of the names of all items in the Global Diagnostics Context.</returns>
        public static ICollection<string> GetNames()
        {
            return GetReadOnlyDict().Keys;
        }

        /// <summary>
        /// Determines whether the Global Diagnostics Context (GDC) dictionary contains the specified key.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>A boolean indicating whether the specified item exists in current thread GDC.</returns>
        public static bool Contains(string item)
        {
            return GetReadOnlyDict().ContainsKey(item);
        }

        /// <summary>
        /// Removes the value with the specified key from the Global Diagnostics Context (GDC) dictionary
        /// </summary>
        /// <param name="item">Item name.</param>
        public static void Remove(string item)
        {
            lock (_lockObject)
            {
                if (_dict.ContainsKey(item))
                {
                    GetWritableDict().Remove(item);
                }
            }
        }

        /// <summary>
        /// Clears the content of the Global Diagnostics Context (GDC) dictionary.
        /// </summary>
        public static void Clear()
        {
            lock (_lockObject)
            {
                if (_dict.Count != 0)
                {
                    GetWritableDict(true).Clear();
                }
            }
        }

        internal static Dictionary<string, object> GetReadOnlyDict()
        {
            var readOnly = _dictReadOnly;
            if (readOnly is null)
            {
                lock (_lockObject)
                {
                    readOnly = _dictReadOnly = _dict;
                }
            }
            return readOnly;
        }

        private static Dictionary<string, object> GetWritableDict(bool clearDictionary = false)
        {
            if (_dictReadOnly != null)
            {
                Dictionary<string, object> newDict = CopyDictionaryOnWrite(clearDictionary);
                _dict = newDict;
                _dictReadOnly = null;
            }
            return _dict;
        }

        private static Dictionary<string, object> CopyDictionaryOnWrite(bool clearDictionary)
        {
            var newDict = new Dictionary<string, object>(clearDictionary ? 0 : _dict.Count + 1, _dict.Comparer);
            if (!clearDictionary)
            {
                // Less allocation with enumerator than Dictionary-constructor
                foreach (var item in _dict)
                    newDict[item.Key] = item.Value;
            }
            return newDict;
        }
    }
}
