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
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A structure to hold define the context.
    /// </summary>
    public interface IContext : IEnumerable<KeyValuePair<string, object>>
    {
        /// <summary>
        /// Returns the keys in the context.
        /// </summary>
        HashSet<string> Keys { get; }

        /// <summary>
        /// Set / Get the key in the context.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object this[string key] { get; set; }

        /// <summary>
        /// Clears the content.
        /// </summary>
        void Clear();

        /// <summary>
        /// Checks whether the specified key exists in the Context.
        /// </summary>
        /// <param name="key">key name.</param>
        /// <returns>A boolean indicating whether the specified key exists in current context.</returns>
        bool Contains(string key);

        /// <summary>
        /// Removes the specified key from the Context.
        /// </summary>
        /// <param name="key">key name.</param>
        void Remove(string key);

        /// <summary>
        /// Gets the Context item.
        /// </summary>
        /// <param name="key">Item name.</param>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> to use when converting the item's value to a string.</param>
        /// <returns>The value of <paramref name="key"/> as a string, if defined; otherwise <see cref="String.Empty"/>.</returns>
        string GetFormatted(string key, IFormatProvider formatProvider);

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        int Count { get; }
    }
}
