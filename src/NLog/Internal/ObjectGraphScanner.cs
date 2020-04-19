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

namespace NLog.Internal
{
    using System;
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
    internal static class ObjectGraphScanner
    {
        /// <summary>
        /// Finds the objects which have attached <see cref="NLogConfigurationItemAttribute"/> which are reachable
        /// from any of the given root objects when traversing the object graph over public properties.
        /// </summary>
        /// <typeparam name="T">Type of the objects to return.</typeparam>
        /// <param name="aggressiveSearch">Also search the properties of the wanted objects.</param>
        /// <param name="rootObjects">The root objects.</param>
        /// <returns>Ordered list of objects implementing T.</returns>
        public static List<T> FindReachableObjects<T>(bool aggressiveSearch, params object[] rootObjects)
            where T : class
        {
            if (InternalLogger.IsTraceEnabled)
            {
                InternalLogger.Trace("FindReachableObject<{0}>:", typeof(T));
            }
            var result = new List<T>();
            var visitedObjects = new HashSet<object>(SingleItemOptimizedHashSet<object>.ReferenceEqualityComparer.Default);
            foreach (var rootObject in rootObjects)
            {
                if (IncludeConfigurationItem(rootObject))
                {
                    ScanProperties(aggressiveSearch, result, rootObject, 0, visitedObjects);
                }
            }
            return result;
        }

        /// <remarks>ISet is not there in .net35, so using HashSet</remarks>
        private static void ScanProperties<T>(bool aggressiveSearch, List<T> result, object o, int level, HashSet<object> visitedObjects)
            where T : class
        {
            if (o == null)
            {
                return;
            }

            if (visitedObjects.Contains(o))
            {
                return;
            }

            var type = o.GetType();

            if (InternalLogger.IsTraceEnabled)
            {
                InternalLogger.Trace("{0}Scanning {1} '{2}'", new string(' ', level), type.Name, o);
            }

            if (o is T t)
            {
                result.Add(t);
                if (!aggressiveSearch)
                {
                    return;
                }
            }

            foreach (var configProp in PropertyHelper.GetAllConfigItemProperties(type))
            {
                if (string.IsNullOrEmpty(configProp.Key))
                    continue;   // Ignore default values

                var propInfo = configProp.Value;
                if (PropertyHelper.IsSimplePropertyType(propInfo.PropertyType))
                    continue;

                var propValue = propInfo.GetValue(o, null);
                if (propValue == null)
                    continue;

                visitedObjects.Add(o);
                ScanPropertyForObject(propInfo, propValue, aggressiveSearch, result, level, visitedObjects);
            }
        }

        private static void ScanPropertyForObject<T>(PropertyInfo prop, object propValue, bool aggressiveSearch, List<T> result, int level, HashSet<object> visitedObjects) where T : class
        {
            if (InternalLogger.IsTraceEnabled)
            {
                InternalLogger.Trace("{0}Scanning Property {1} '{2}' {3}", new string(' ', level + 1), prop.Name, propValue.ToString(), prop.PropertyType.Namespace);
            }

            if (propValue is IEnumerable enumerable)
            {
                var list = ConvertEnumerableToList(enumerable);
                if (list.Count > 0 && !visitedObjects.Contains(list))
                {
                    visitedObjects.Add(list);
                    ScanPropertiesList(list, aggressiveSearch, result, level + 1, visitedObjects);
                }
            }
            else
            {
                // .NET native doesn't always allow reflection of System-types (Ex. Encoding)
                if (IncludeConfigurationItem(propValue, prop.PropertyType))
                {
                    ScanProperties(aggressiveSearch, result, propValue, level + 1, visitedObjects);
                }
            }
        }

        private static void ScanPropertiesList<T>(IList list, bool aggressiveSearch, List<T> result, int level, HashSet<object> visitedObjects) where T : class
        {
            for (int i = 0; i < list.Count; i++)
            {
                var element = list[i];
                if (IncludeConfigurationItem(element))
                {
                    ScanProperties(aggressiveSearch, result, element, level, visitedObjects);
                }
            }
        }

        private static IList ConvertEnumerableToList(IEnumerable enumerable)
        {
            if (enumerable is ICollection collection && collection.Count == 0)
            {
                return ArrayHelper.Empty<object>();
            }

            if (enumerable is IList list)
            {
                if (!list.IsReadOnly)
                {
                    List<object> elements = new List<object>(list.Count);
                    lock (list.SyncRoot)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            elements.Add(list[i]);
                        }
                    }
                    return elements;
                }

                return list;
            }

            //new list to prevent: Collection was modified after the enumerator was instantiated.
            //note .Cast is tread-unsafe! But at least it isn't a ICollection / IList
            return enumerable.Cast<object>().ToList();
        }

        private static bool IncludeConfigurationItem(object item, Type propertyType = null)
        {
            propertyType = propertyType ?? item?.GetType();
            if (propertyType == null)
                return false;

            if (PropertyHelper.IsConfigurationItemType(propertyType))
                return true;

            var itemType = item?.GetType();
            if (itemType != null && itemType != propertyType && PropertyHelper.IsConfigurationItemType(itemType))
                return true;

            return false;
        }
    }
}