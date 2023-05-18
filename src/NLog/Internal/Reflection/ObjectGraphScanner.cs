// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
        /// <param name="configFactory">Configuration Reflection Helper</param>
        /// <param name="aggressiveSearch">Also search the properties of the wanted objects.</param>
        /// <param name="rootObjects">The root objects.</param>
        /// <returns>Ordered list of objects implementing T.</returns>
        public static List<T> FindReachableObjects<T>(ConfigurationItemFactory configFactory, bool aggressiveSearch, params object[] rootObjects)
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
                if (PropertyHelper.IsConfigurationItemType(configFactory, rootObject?.GetType()))
                {
                    ScanProperties(configFactory, aggressiveSearch, rootObject, result, 0, visitedObjects);
                }
            }
            return result;
        }

        private static void ScanProperties<T>(ConfigurationItemFactory configFactory, bool aggressiveSearch, object targetObject, List<T> result, int level, HashSet<object> visitedObjects)
            where T : class
        {
            if (targetObject is null)
            {
                return;
            }

            if (visitedObjects.Contains(targetObject))
            {
                return;
            }

            if (targetObject is T t)
            {
                result.Add(t);
                if (!aggressiveSearch)
                {
                    return;
                }
            }

            var type = targetObject.GetType();
            if (InternalLogger.IsTraceEnabled)
            {
                InternalLogger.Trace("{0}Scanning {1} '{2}'", new string(' ', level), type.Name, targetObject);
            }

            foreach (var configProp in PropertyHelper.GetAllConfigItemProperties(configFactory, type))
            {
                if (string.IsNullOrEmpty(configProp.Key))
                    continue;   // Ignore default values

                if (!PropertyHelper.IsConfigurationItemType(configFactory, configProp.Value.PropertyType))
                    continue;

                var propInfo = configProp.Value;
                var propValue = propInfo.GetValue(targetObject, null);
                if (propValue is null)
                    continue;

                visitedObjects.Add(targetObject);
                ScanPropertyForObject(configFactory, aggressiveSearch, propValue, propInfo, result, level, visitedObjects);
            }
        }

        private static void ScanPropertyForObject<T>(ConfigurationItemFactory configFactory, bool aggressiveSearch, object propValue, PropertyInfo prop, List<T> result, int level, HashSet<object> visitedObjects) where T : class
        {
            if (InternalLogger.IsTraceEnabled)
            {
                InternalLogger.Trace("{0}Scanning Property {1} '{2}' {3}", new string(' ', level + 1), prop.Name, propValue, prop.PropertyType);
            }

            if (propValue is IEnumerable enumerable)
            {
                var propListValue = ConvertEnumerableToList(enumerable, visitedObjects);
                if (propListValue.Count > 0)
                {
                    ScanPropertiesList(configFactory, aggressiveSearch, propListValue, result, level + 1, visitedObjects);
                }
            }
            else
            {
                ScanProperties(configFactory, aggressiveSearch, propValue, result, level + 1, visitedObjects);
            }
        }

        private static void ScanPropertiesList<T>(ConfigurationItemFactory configFactory, bool aggressiveSearch, IList list, List<T> result, int level, HashSet<object> visitedObjects) where T : class
        {
            var firstElement = list[0];
            if (firstElement is null || PropertyHelper.IsConfigurationItemType(configFactory, firstElement.GetType()))
            {
                ScanProperties(configFactory, aggressiveSearch, firstElement, result, level, visitedObjects);

                for (int i = 1; i < list.Count; i++)
                {
                    var element = list[i];
                    ScanProperties(configFactory, aggressiveSearch, element, result, level, visitedObjects);
                }
            }
        }

        private static IList ConvertEnumerableToList(IEnumerable enumerable, HashSet<object> visitedObjects)
        {
            if (enumerable is ICollection collection && collection.Count == 0)
            {
                return ArrayHelper.Empty<object>();
            }

            if (visitedObjects.Contains(enumerable))
            {
                return ArrayHelper.Empty<object>();
            }

            visitedObjects.Add(enumerable);

            if (enumerable is IList list)
            {
                if (!list.IsReadOnly && !(list is Array))
                {
                    // Protect against collection was modified
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
    }
}