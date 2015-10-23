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

namespace NLog.Contexts
{
#if NET4_0 || NET4_5
    using NLog.Internal;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Remoting.Messaging;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Async version of Thread Context - a logical context structure that keeps a dictionary
    /// of strings and provides methods to output them in layouts.  Allows for maintaining state across
    /// asynchronous tasks and call contexts.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class LogicalThreadContext : IContext
    {
        private const string LogicalThreadDictionaryKey = "NLog.AsyncableMappedDiagnosticsContext";
        private static IContext currentInstance;
        private static readonly object syncRoot = new object();
        private HashSet<string> keys;

        private IDictionary<string, object> LogicalThreadDictionary
        {
            get
            {
                return CallContext.LogicalGetData(LogicalThreadDictionaryKey) as ConcurrentDictionary<string, object>;
            }
            set
            {
                lock (syncRoot)
                {
                    CallContext.LogicalSetData(LogicalThreadDictionaryKey, value);
                }
            }
        }

        /// <summary>
        /// Gets the Mapped Context key.
        /// </summary>
        /// <param name="key">key name.</param>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> to use when converting the key's value to a string.</param>
        /// <returns>The value of <paramref name="key"/> as a string, if defined; otherwise <see cref="String.Empty"/>.</returns>
        public string GetFormatted(string key, IFormatProvider formatProvider)
        {
            return FormatHelper.ConvertToString(this.TryGet(key), formatProvider);
        }

        /// <summary>
        /// Sets the current logical context key to the specified value.
        /// </summary>
        /// <param name="key">Item name.</param>
        /// <param name="value">Item value.</param>
        public void Set(string key, object value)
        {
            this[key] = value;
        }

        /// <summary>
        /// Checks whether the specified <paramref name="key"/> exists in current logical context.
        /// </summary>
        /// <param name="key">Item name.</param>
        /// <returns>A boolean indicating whether the specified <paramref name="key"/> exists in current logical context.</returns>
        public bool Contains(string key)
        {
            lock (syncRoot)
            {
                return this.LogicalThreadDictionary.ContainsKey(key);
            }
        }

        /// <summary>
        /// Removes the specified <paramref name="key"/> from current logical context.
        /// </summary>
        /// <param name="key">Item name.</param>
        public void Remove(string key)
        {
            lock (syncRoot)
            {
                this.LogicalThreadDictionary.Remove(key);
            }
        }

        /// <summary>
        /// Clears the content of current logical context.
        /// </summary>
        public void Clear()
        {
            lock (syncRoot)
            {
                this.LogicalThreadDictionary.Clear();
                this.keys.Clear();
            }
        }

        /// <summary>
        /// Gets the Mapped Context named key.
        /// </summary>
        /// <param name="key">key name.</param>
        /// <returns>The key value, if defined; otherwise <c>null</c>.</returns>
        private object TryGet(string key)
        {
            lock (syncRoot)
            {
                object o;
                if (!this.LogicalThreadDictionary.TryGetValue(key, out o))
                    o = null;

                return o;
            }
        }

        private LogicalThreadContext()
        {
            keys = new HashSet<string>();
            LogicalThreadDictionary = new ConcurrentDictionary<string, object>();
        }
        /// <summary>
        /// Current instance of the context.
        /// </summary>
        public static IContext Instance
        {
            get
            {
                if (currentInstance == null)
                {
                    lock (syncRoot)
                    {
                        if (currentInstance == null)
                        {
                            currentInstance = new LogicalThreadContext();
                            return currentInstance;
                        }
                    }
                }
                return currentInstance;
            }
        }

        /// <summary>
        /// Returns the keys in the context.
        /// </summary>
        public HashSet<string> Keys
        {
            get
            {
                return keys;
            }
        }

        /// <summary>
        /// Set / Get the key in the context.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The key value, if defined; otherwise <c>null</c>.</returns>
        public object this[string key]
        {
            get
            {
                return this.TryGet(key);
            }
            set
            {
                lock (syncRoot)
                {
                    this.LogicalThreadDictionary[key] = value;

                    if (!keys.Contains(key))
                        keys.Add(key);
                }
            }
        }
        //TODO SOLUTION IN PROGRESS.
        /// <summary>
        /// Returns an enumerator that iterates through the Internal Dictionary structure.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the Internal Dictionary structure.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
#endif
}
