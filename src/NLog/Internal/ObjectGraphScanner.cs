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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using NLog.Common;
    using NLog.Config;

    /// <summary>
    /// Scans (breadth-first) the object graph following all the edges whose are 
    /// instances have <see cref="NLogConfigurationItemAttribute"/> attached and returns 
    /// all objects implementing a specified interfaces.
    /// </summary>
    internal class ObjectGraphScanner
    {
        /// <summary>
        /// Finds the objects which have attached <see cref="NLogConfigurationItemAttribute"/> which are reachable
        /// from any of the given root objects when traversing the object graph over public properties.
        /// </summary>
        /// <typeparam name="T">Type of the objects to return.</typeparam>
        /// <param name="rootObjects">The root objects.</param>
        /// <returns>Ordered list of objects implementing T.</returns>
        public static List<T> FindReachableObjects<T>(params object[] rootObjects)
            where T : class
        {
            InternalLogger.Trace("FindReachableObject<{0}>:", typeof(T));
            var result = new List<T>();
            var visitedObjects = new HashSet<object>();

            foreach (var rootObject in rootObjects)
            {
                ScanProperties(result, rootObject, 0, visitedObjects);
            }

            return result.ToList();
        }

        /// <remarks>ISet is not there in .net35, so using HashSet</remarks>
        private static void ScanProperties<T>(List<T> result, object o, int level, HashSet<object> visitedObjects)
            where T : class
        {
            if (o == null)
            {
                return;
            }

            var type = o.GetType();
            if (!type.IsDefined(typeof(NLogConfigurationItemAttribute), true))
            {
                return;
            }

            if (visitedObjects.Contains(o))
            {
                return;
            }

            visitedObjects.Add(o);

            var t = o as T;
            if (t != null)
            {
                result.Add(t);
            }

            if (InternalLogger.IsTraceEnabled)
            {
                InternalLogger.Trace("{0}Scanning {1} '{2}'", new string(' ', level), type.Name, o);
            }

            foreach (PropertyInfo prop in PropertyHelper.GetAllReadableProperties(type))
            {
                if (prop.PropertyType.IsPrimitive || prop.PropertyType.IsEnum || prop.PropertyType == typeof(string) || prop.IsDefined(typeof(NLogConfigurationIgnorePropertyAttribute), true))
                {
                    continue;
                }

                object value = prop.GetValue(o, null);
                if (value == null)
                {
                    continue;
                }

                var list = value as IList;
                if (list != null)
                {
                    //try first icollection for syncroot
                    List<object> elements;
                    lock (list.SyncRoot)
                    {
                        elements = new List<object>(list.Count);
                        //no foreach. Even .Cast can lead to  Collection was modified after the enumerator was instantiated.
                        for (int i = 0; i < list.Count; i++)
                        {
                            var item = list[i];
                            elements.Add(item);
                        }
                    }
                    ScanPropertiesList(result, elements, level + 1, visitedObjects);
                }
                else
                {
                    var enumerable = value as IEnumerable;
                    if (enumerable != null)
                    {
                        //cast to list otherwhise possible:  Collection was modified after the enumerator was instantiated.
                        var elements = enumerable as IList<object> ?? enumerable.Cast<object>().ToList();

                        ScanPropertiesList(result, elements, level + 1, visitedObjects);
                    }
                    else
                    {
                        ScanProperties(result, value, level + 1, visitedObjects);
                    }
                }
            }
        }

        private static void ScanPropertiesList<T>(List<T> result, IEnumerable<object> elements, int level, HashSet<object> visitedObjects) where T : class
        {
            foreach (object element in elements)
            {
                ScanProperties(result, element, level, visitedObjects);
            }
        }
    }
}