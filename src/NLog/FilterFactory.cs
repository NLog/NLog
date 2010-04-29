// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Text;
using System.Collections;
using System.Reflection;
using System.Globalization;

using NLog.Internal;

namespace NLog
{
    /// <summary>
    /// A factory of logging filters. Creates new filters based on their names.
    /// </summary>
    public sealed class FilterFactory
    {
        private static TypeDictionary _filters = new TypeDictionary();

        static FilterFactory()
        {
            foreach (Assembly a in ExtensionUtils.GetExtensionAssemblies())
            {
                AddFiltersFromAssembly(a, "");
            }
        }

        private FilterFactory(){}

        /// <summary>
        /// Removes all filter information from the factory.
        /// </summary>
        public static void Clear()
        {
            _filters.Clear();
        }

        /// <summary>
        /// Scans the specified assembly for types marked with <see cref="FilterAttribute" /> and adds
        /// them to the factory. Optionally it prepends the specified text to filter names to avoid
        /// naming collisions.
        /// </summary>
        /// <param name="theAssembly">The assembly to be scanned for filters.</param>
        /// <param name="prefix">The prefix to be prepended to filter names.</param>
        public static void AddFiltersFromAssembly(Assembly theAssembly, string prefix)
        {
            try
            {
                InternalLogger.Debug("AddFiltersFromAssembly('{0}')", theAssembly.FullName);
                foreach (Type t in theAssembly.GetTypes())
                {
                    FilterAttribute[]attributes = (FilterAttribute[])t.GetCustomAttributes(typeof(FilterAttribute), false);
                    if (attributes != null)
                    {
                        foreach (FilterAttribute attr in attributes)
                        {
                            if (PlatformDetector.IsSupportedOnCurrentRuntime(t))
                            {
                                AddFilter(prefix + attr.Name, t);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Failed to add filters from '" + theAssembly.FullName + "': {0}", ex);
            }
            
        }

        /// <summary>
        /// Registers the specified filter type to the factory under a specified name.
        /// </summary>
        /// <param name="name">The name of the filter (e.g. <code>whenEquals</code> or <code>whenContains</code>)</param>
        /// <param name="t">The type of the new filter</param>
        /// <remarks>
        /// The name specified in the name parameter can then be used
        /// to create filters.
        /// </remarks>
        public static void AddFilter(string name, Type t)
        {
            InternalLogger.Trace("Registering filter {0} for type '{1}')", name, t.FullName);
            _filters[name.ToLower(CultureInfo.InvariantCulture)] = t;
        }

        /// <summary>
        /// Creates the filter object based on its filter name.
        /// </summary>
        /// <param name="name">The name of the filter (e.g. <code>whenEquals</code> or <code>whenNotEqual</code>)</param>
        /// <returns>A new instance of the <see cref="Filter"/> object.</returns>
        public static Filter CreateFilter(string name)
        {
            Type t = _filters[name.ToLower(CultureInfo.InvariantCulture)];
            if (t != null)
            {
                Filter la = FactoryHelper.CreateInstance(t) as Filter;
                if (la != null)
                    return la;
            }
            throw new ArgumentException("Filter " + name + " not found.");
        }

        /// <summary>
        /// Collection of filter types added to the factory.
        /// </summary>
        public static ICollection FilterTypes
        {
            get { return _filters.Values; }
        }
    }
}
