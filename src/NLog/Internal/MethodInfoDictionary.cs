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
using System.Collections;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Text;

using NLog.Config;

namespace NLog.Internal
{
    // CLOVER:OFF

    /// <summary>
    /// A dictionary with keys of type string and values of type MethodInfo
    /// </summary>
    internal class MethodInfoDictionary: System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the MethodInfoDictionary class
        /// </summary>
        public MethodInfoDictionary()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the MethodInfo associated with the given string
        /// </summary>
        /// <param name="key">
        /// The string whose value to get or set.
        /// </param>
        public virtual MethodInfo this[string key]
        {
            get { return (MethodInfo)this.Dictionary[key]; }
            set { this.Dictionary[key] = value; }
        }

        /// <summary>
        /// Adds an element with the specified key and value to this MethodInfoDictionary.
        /// </summary>
        /// <param name="key">
        /// The string key of the element to add.
        /// </param>
        /// <param name="value">
        /// The MethodInfo value of the element to add.
        /// </param>
        public virtual void Add(string key, MethodInfo value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this MethodInfoDictionary contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The string key to locate in this MethodInfoDictionary.
        /// </param>
        /// <returns>
        /// true if this MethodInfoDictionary contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(string key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this MethodInfoDictionary contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The string key to locate in this MethodInfoDictionary.
        /// </param>
        /// <returns>
        /// true if this MethodInfoDictionary contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(string key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this MethodInfoDictionary contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The MethodInfo value to locate in this MethodInfoDictionary.
        /// </param>
        /// <returns>
        /// true if this MethodInfoDictionary contains an element with the specified value;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsValue(MethodInfo value)
        {
            foreach (MethodInfo item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from this MethodInfoDictionary.
        /// </summary>
        /// <param name="key">
        /// The string key of the element to remove.
        /// </param>
        public virtual void Remove(string key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this MethodInfoDictionary.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get { return this.Dictionary.Keys; }
        }

        /// <summary>
        /// Gets a collection containing the values in this MethodInfoDictionary.
        /// </summary>
        public virtual System.Collections.ICollection Values
        {
            get { return this.Dictionary.Values; }
        }
    }
}
