// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

namespace NLog.Filters
{
    public sealed class FilterFactory
    {
        private static TypeDictionary _filters = new TypeDictionary();

        static FilterFactory()
        {
            Clear();
            AddDefaultFilters();
        }

        private FilterFactory()
        {
        }

        public static void Clear() {
            _filters.Clear();
        }

        public static void AddFiltersFromAssembly(Assembly theAssembly, string prefix) {
            InternalLogger.Debug("AddFiltersFromAssembly('{0}')", theAssembly.FullName);
            foreach (Type t in theAssembly.GetTypes()) {
                FilterAttribute[] attributes = (FilterAttribute[])t.GetCustomAttributes(typeof(FilterAttribute), false);
                if (attributes != null) {
                    foreach (FilterAttribute attr in attributes) {
                        AddFilter(prefix + attr.Name, t);
                    }
                }
            }
        }
        private static void AddDefaultFilters() {
            AddFiltersFromAssembly(typeof(FilterFactory).Assembly, String.Empty);
        }

        public static void AddFilter(string name, Type t) {
            InternalLogger.Debug("AddFilter('{0}','{1}')", name, t.FullName);
            _filters[name.ToLower(CultureInfo.InvariantCulture)] = t;
        }

        public static Filter CreateFilter(string name) {
            Type t = _filters[name.ToLower(CultureInfo.InvariantCulture)];
            if (t != null) {
                object o = FactoryHelper.CreateInstance(t);
                if (o is Filter) {
                    Filter la = (Filter)o;
                    return la;
                }
                else
                    throw new ArgumentException("Filter " + name + " not found.");
            }
            throw new ArgumentException("Filter " + name + " not found.");
        }
    }
}
