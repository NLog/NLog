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
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Global Diagnostics Context - a dictionary structure to hold per-application-instance values.
    /// </summary>
    public static class GlobalDiagnosticsContext
    {
        private static Dictionary<string, object> dict = new Dictionary<string, object>();

        /// <summary>
        /// Sets the Global Diagnostics Context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        public static void Set(string item, string value)
        {
            lock (dict)
            {
                dict[item] = value;
            }
        }

        /// <summary>
        /// Sets the Global Diagnostics Context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        public static void Set(string item, object value)
        {
            lock (dict)
            {
                dict[item] = value;
            }
        }

        /// <summary>
        /// Gets the Global Diagnostics Context named item.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>The item value of string.Empty if the value is not present.</returns>
        public static string Get(string item)
        {
            return RenderContextObject(GetObject(item));
        }

        /// <summary>
        /// Gets the Global Diagnostics Context named item.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>The item value, if defined; otherwise <c>null</c>.</returns>
        public static object GetObject(string item)
        {
            lock (dict)
            {
                object o;
                if (!dict.TryGetValue(item, out o))
                    o = null;

                return o;
            }
        }

        /// <summary>
        /// Checks whether the specified item exists in the Global Diagnostics Context.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>A boolean indicating whether the specified item exists in current thread GDC.</returns>
        public static bool Contains(string item)
        {
            lock (dict)
            {
                return dict.ContainsKey(item);
            }
        }

        /// <summary>
        /// Removes the specified item from the Global Diagnostics Context.
        /// </summary>
        /// <param name="item">Item name.</param>
        public static void Remove(string item)
        {
            lock (dict)
            {
                dict.Remove(item);
            }
        }

        /// <summary>
        /// Clears the content of the GDC.
        /// </summary>
        public static void Clear()
        {
            lock (dict)
            {
                dict.Clear();
            }
        }

        internal static string RenderContextObject ( object o )
        {
            // TODO: Should consider if Configuration.DefaultCultureInfo should be used here instead of
            //       null. Currently LogEventInfo uses Culture.CurrentCulture rather than deferring to the
            //       Configuration.DefaultCultureInfo, which I suspect is a bug, but I don't know that 
            //       yet. In any case, there is no LogEventInfo instance available here, so we either need
            //       to use the current culture or defer to Configuration.DefaultCultureInfo. Currently
            //       I've choosen to follow LogEventInfo and use whatever the current culture is.

            return String.Format(null, "{0}", o);
        }
    }
}
