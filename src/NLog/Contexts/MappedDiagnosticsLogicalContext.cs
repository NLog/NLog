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
    [Obsolete("Replaced by ScopeContext.PushProperty or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
    public static class MappedDiagnosticsLogicalContext
    {
        /// <summary>
        /// Gets the current logical context named item, as <see cref="string"/>.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>The value of <paramref name="item"/>, if defined; otherwise <see cref="String.Empty"/>.</returns>
        /// <remarks>If the value isn't a <see cref="string"/> already, this call locks the <see cref="LogFactory"/> for reading the <see cref="Config.LoggingConfiguration.DefaultCultureInfo"/> needed for converting to <see cref="string"/>. </remarks>
        [Obsolete("Replaced by ScopeContext.TryGetProperty. Marked obsolete on NLog 5.0")]
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
        [Obsolete("Replaced by ScopeContext.TryGetProperty. Marked obsolete on NLog 5.0")]
        public static string Get(string item, IFormatProvider formatProvider)
        {
            return FormatHelper.ConvertToString(GetObject(item), formatProvider);
        }

        /// <summary>
        /// Gets the current logical context named item, as <see cref="object"/>.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>The value of <paramref name="item"/>, if defined; otherwise <c>null</c>.</returns>
        [Obsolete("Replaced by ScopeContext.TryGetProperty. Marked obsolete on NLog 5.0")]
        public static object GetObject(string item)
        {
            if (ScopeContext.TryGetProperty(item, out var value))
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
        [Obsolete("Replaced by ScopeContext.PushProperty or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
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
        [Obsolete("Replaced by ScopeContext.PushProperty or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
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
        [Obsolete("Replaced by ScopeContext.PushProperty or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
        public static IDisposable SetScoped<T>(string item, T value)
        {
            return ScopeContext.PushProperty(item, value);
        }

#if !NET35 && !NET40
        /// <summary>
        /// Updates the current logical context with multiple items in single operation
        /// </summary>
        /// <param name="items">.</param>
        /// <returns>>An <see cref="IDisposable"/> that can be used to remove the item from the current logical context (null if no items).</returns>
        [Obsolete("Replaced by ScopeContext.PushProperties or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
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
        [Obsolete("Replaced by ScopeContext.PushProperty or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
        public static void Set(string item, string value)
        {
            Set<string>(item, value);
        }

        /// <summary>
        /// Sets the current logical context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        [Obsolete("Replaced by ScopeContext.PushProperty or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
        public static void Set(string item, object value)
        {
            Set<object>(item, value);
        }

        /// <summary>
        /// Sets the current logical context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        [Obsolete("Replaced by ScopeContext.PushProperty or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
        public static void Set<T>(string item, T value)
        {
            ScopeContext.SetMappedContextLegacy(item, value);
        }

        /// <summary>
        /// Returns all item names
        /// </summary>
        /// <returns>A collection of the names of all items in current logical context.</returns>
        [Obsolete("Replaced by ScopeContext.GetAllProperties. Marked obsolete on NLog 5.0")]
        public static ICollection<string> GetNames()
        {
            return ScopeContext.GetKeysMappedContextLegacy();
        }

        /// <summary>
        /// Checks whether the specified <paramref name="item"/> exists in current logical context.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>A boolean indicating whether the specified <paramref name="item"/> exists in current logical context.</returns>
        [Obsolete("Replaced by ScopeContext.TryGetProperty. Marked obsolete on NLog 5.0")]
        public static bool Contains(string item)
        {
            return ScopeContext.TryGetProperty(item, out var _);
        }

        /// <summary>
        /// Removes the specified <paramref name="item"/> from current logical context.
        /// </summary>
        /// <param name="item">Item name.</param>
        [Obsolete("Replaced by dispose of return value from ScopeContext.PushProperty. Marked obsolete on NLog 5.0")]
        public static void Remove(string item)
        {
            ScopeContext.RemoveMappedContextLegacy(item);
        }

        /// <summary>
        /// Clears the content of current logical context.
        /// </summary>
        [Obsolete("Replaced by ScopeContext.Clear(). Marked obsolete on NLog 5.0")]
        public static void Clear()
        {
            Clear(true);
        }

        /// <summary>
        /// Clears the content of current logical context.
        /// </summary>
        /// <param name="free">Free the full slot.</param>
        [Obsolete("Replaced by ScopeContext.Clear(). Marked obsolete on NLog 5.0")]
        public static void Clear(bool free)
        {
            ScopeContext.ClearMappedContextLegacy();
        }
    }
}