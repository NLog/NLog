// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NLog.Config;

namespace NLog.Internal
{
    /// <summary>
    /// Scans (breadth-first) the object graph following all the edges whose are 
    /// instances implement <see cref="INLogConfigurationItem"/> and returns all objects implementing a specified interfaces.
    /// </summary>
    /// <typeparam name="T">Type of the objects to return.</typeparam>
    internal class ObjectGraphScanner<T>
        where T : class
    {
        private readonly Dictionary<object, bool> visitedObjects = new Dictionary<object, bool>();
        private readonly Queue<INLogConfigurationItem> queue = new Queue<INLogConfigurationItem>();

        /// <summary>
        /// Scans the graph starting from the list of roots specified with AddRoot() calls.
        /// </summary>
        /// <returns>Ordered list of objects implementing T.</returns>
        public T[] Scan()
        {
            var result = new List<T>();

            while (this.queue.Count > 0)
            {
                INLogConfigurationItem o = this.queue.Dequeue();
                T t = o as T;
                if (t != null)
                {
                    result.Add(t);
                }

                // Console.WriteLine("Scanning {0}", o);
                this.ScanProperties(o);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Adds the specified root object.
        /// </summary>
        /// <param name="rootObject">The root object.</param>
        public void AddRoot(INLogConfigurationItem rootObject)
        {
            this.Enqueue(rootObject);
        }

        private void Enqueue(INLogConfigurationItem o)
        {
            if (o == null)
            {
                return;
            }

            if (!this.visitedObjects.ContainsKey(o))
            {
                this.visitedObjects.Add(o, true);
                this.queue.Enqueue(o);
            }
        }

        private void ScanProperties(INLogConfigurationItem o)
        {
            foreach (PropertyInfo prop in PropertyHelper.GetAllReadableProperties(o.GetType()))
            {
                if (prop.PropertyType.IsPrimitive || prop.PropertyType.IsEnum || prop.PropertyType == typeof(string))
                {
                    continue;
                }

                object value = prop.GetValue(o, null);
                if (value == null)
                {
                    continue;
                }

                var enumerable = value as IEnumerable;
                if (enumerable != null)
                {
                    foreach (object element in enumerable)
                    {
                        this.Enqueue(element as INLogConfigurationItem);
                    }

                    continue;
                }

                this.Enqueue(value as INLogConfigurationItem);
            }
        }
    }
}
