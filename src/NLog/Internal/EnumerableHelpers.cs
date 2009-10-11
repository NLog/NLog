using System.Collections.Generic;
using System.Collections;

namespace NLog.Internal
{
    /// <summary>
    /// LINQ-like helpers (cannot use LINQ because we must work with .NET 2.0 profile)
    /// </summary>
    internal static class EnumerableHelpers
    {
        /// <summary>
        /// Filters the given enumerable to return only items of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns>Items of specified type.</returns>
        public static IEnumerable<T> OfType<T>(IEnumerable enumerable)
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
        /// <typeparam name="T">Type of enumerable item.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns>Reversed enumerable</returns>
        public static IEnumerable<T> Reverse<T>(IEnumerable<T> enumerable)
            where T : class
        {
            List<T> tmp = new List<T>(enumerable);
            tmp.Reverse();
            return tmp;
        }
    }
}
