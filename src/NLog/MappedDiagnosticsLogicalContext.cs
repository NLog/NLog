// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
#if NET4_5
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.Remoting.Messaging;

    /// <summary>
    /// Async version of Mapped Diagnostics Context - a logical context structure that keeps a dictionary
    /// of strings and provides methods to output them in layouts.  Allows for maintaining state across
    /// asynchronous tasks and call contexts.
    /// Mostly for compatibility with log4net (log4net.ThreadLogicalContext).
    /// </summary>
    /// <remarks>
    /// Ideally, these changes should be incorporated as a new version of the MappedDiagnosticsContext class in the original
    /// NLog library so that state can be maintained for multiple threads in asynchronous situations.
    /// </remarks>
    public static class MappedDiagnosticsLogicalContext
    {
        private const string LogicalThreadDictionaryKey = "NLog.AsyncableMappedDiagnosticsContext";

        private static IDictionary<string, string> LogicalThreadDictionary
        {
            get
            {
                var dictionary = CallContext.LogicalGetData(LogicalThreadDictionaryKey) as ConcurrentDictionary<string, string>;
                if (dictionary == null)
                {
                    dictionary = new ConcurrentDictionary<string, string>();
                    CallContext.LogicalSetData(LogicalThreadDictionaryKey, dictionary);
                }
                return dictionary;
            }
        }

        /// <summary>
        /// Gets the current logical context named item.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>The item value of string.Empty if the value is not present.</returns>
        public static string Get(string item)
        {
            string value;

            if (!LogicalThreadDictionary.TryGetValue(item, out value))
            {
                value = string.Empty;
            }

            return value;
        }

        /// <summary>
        /// Sets the current logical context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        public static void Set(string item, string value)
        {
            LogicalThreadDictionary[item] = value;
        }

        /// <summary>
        /// Checks whether the specified item exists in current logical context.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>A boolean indicating whether the specified item exists in current thread MDC.</returns>
        public static bool Contains(string item)
        {
            return LogicalThreadDictionary.ContainsKey(item);
        }

        /// <summary>
        /// Removes the specified item from current logical context.
        /// </summary>
        /// <param name="item">Item name.</param>
        public static void Remove(string item)
        {
            LogicalThreadDictionary.Remove(item);
        }

        /// <summary>
        /// Clears the content of current logical context.
        /// </summary>
        public static void Clear()
        {
            LogicalThreadDictionary.Clear();
        }
    }
#endif
}