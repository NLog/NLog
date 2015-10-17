
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

namespace NLog.Context
{
    using NLog.Internal;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Mapped Context - a thread-local structure that keeps a dictionary of strings and provides methods to output them in layouts.
    /// </summary>
    public class ThreadContext : IContext
    {
        private readonly object dataSlot = null;
        private static IContext currentInstance = null;
        private static readonly object syncRoot = new object();
        private Dictionary<string, object> dict;


        private ThreadContext()
        {
            dataSlot = ThreadLocalStorageHelper.AllocateDataSlot();
            dict = ThreadLocalStorageHelper.GetDataForSlot<Dictionary<string, object>>(dataSlot);
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
                            currentInstance = new ThreadContext();
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
                return new HashSet<string>(this.dict.Keys);
            }
        }


        /// <summary>
        /// Set / Get the key in the context.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
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
                    this.dict[key] = value;
                }
            }
        }

        /// <summary>
        /// Clears the content of current thread Mapped Context.
        /// </summary>
        public void Clear()
        {
            lock (syncRoot)
            {
                this.dict.Clear();
            }
        }

        /// <summary>
        /// Checks whether the specified key exists in current thread Mapped Context.
        /// </summary>
        /// <param name="key">item name.</param>
        /// <returns>A boolean indicating whether the specified <paramref name="key"/> exists in current thread Mapped Context.</returns>
        public bool Contains(string key)
        {
            lock (syncRoot)
            {
                return this.dict.ContainsKey(key);
            }
        }

        /// <summary>
        /// Removes the specified <paramref name="key"/> from current thread Mapped Context.
        /// </summary>
        /// <param name="key">key name.</param>
        public void Remove(string key)
        {
            lock (syncRoot)
            {
                this.dict.Remove(key);
            }
        }

        /// <summary>
        /// Sets the current thread Mapped Context key to the specified value.
        /// </summary>
        /// <param name="key">key name.</param>
        /// <param name="value">key value.</param>
        public void Set(string key, object value)
        {
            lock (syncRoot)
            {
                this.dict[key] = value;
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
                if (!this.dict.TryGetValue(key, out o))
                    o = null;

                return o;
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
    }
}
