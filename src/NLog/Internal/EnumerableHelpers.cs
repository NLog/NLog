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

namespace NLog.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// LINQ-like helpers (cannot use LINQ because we must work with .NET 2.0 profile).
    /// </summary>
    internal static class EnumerableHelpers
    {
        /// <summary>
        /// Filters the given enumerable to return only items of the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the item.
        /// </typeparam>
        /// <param name="enumerable">
        /// The enumerable.
        /// </param>
        /// <returns>
        /// Items of specified type.
        /// </returns>
        public static IEnumerable<T> OfType<T>(this IEnumerable enumerable)
            where T : class
        {
            foreach (object o in enumerable)
            {
                T t = o as T;
                if (t != null)
                {
                    yield return t;
                }
            }
        }

        /// <summary>
        /// Reverses the specified enumerable.
        /// </summary>
        /// <typeparam name="T">
        /// Type of enumerable item.
        /// </typeparam>
        /// <param name="enumerable">
        /// The enumerable.
        /// </param>
        /// <returns>
        /// Reversed enumerable.
        /// </returns>
        public static IEnumerable<T> Reverse<T>(this IEnumerable<T> enumerable)
            where T : class
        {
            List<T> tmp = new List<T>(enumerable);
            tmp.Reverse();
            return tmp;
        }

        /// <summary>
        /// Determines is the given predicate is met by any element of the enumerable.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>True if predicate returns true for any element of the collection, false otherwise.</returns>
        public static bool Any<T>(this IEnumerable<T> enumerable, Predicate<T> predicate)
        {
            foreach (var t in enumerable)
            {
                if (predicate(t))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Converts the enumerable to list.
        /// </summary>
        /// <typeparam name="T">Type of the list element.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns>List of elements.</returns>
        public static List<T> ToList<T>(this IEnumerable<T> enumerable)
        {
            return new List<T>(enumerable);
        }
    }
}
