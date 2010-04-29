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

#if NETCF_1_0

using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Text;

namespace System.Collections.Specialized
{
    /// <summary>
    /// A dictionary with keys of type string and values of type String
    /// </summary>
    internal class StringDictionary: System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the StringDictionary class
        /// </summary>
        public StringDictionary()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the String associated with the given string
        /// </summary>
        /// <param name="key">
        /// The string whose value to get or set.
        /// </param>
        public virtual String this[string key]
        {
            get { return (String)this.Dictionary[key]; }
            set { this.Dictionary[key] = value; }
        }

        /// <summary>
        /// Adds an element with the specified key and value to this StringDictionary.
        /// </summary>
        /// <param name="key">
        /// The string key of the element to add.
        /// </param>
        /// <param name="value">
        /// The String value of the element to add.
        /// </param>
        public virtual void Add(string key, String value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this StringDictionary contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The string key to locate in this StringDictionary.
        /// </param>
        /// <returns>
        /// true if this StringDictionary contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(string key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringDictionary contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The string key to locate in this StringDictionary.
        /// </param>
        /// <returns>
        /// true if this StringDictionary contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(string key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringDictionary contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The String value to locate in this StringDictionary.
        /// </param>
        /// <returns>
        /// true if this StringDictionary contains an element with the specified value;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsValue(String value)
        {
            foreach (String item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from this StringDictionary.
        /// </summary>
        /// <param name="key">
        /// The string key of the element to remove.
        /// </param>
        public virtual void Remove(string key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this StringDictionary.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get { return this.Dictionary.Keys; }
        }

        /// <summary>
        /// Gets a collection containing the values in this StringDictionary.
        /// </summary>
        public virtual System.Collections.ICollection Values
        {
            get { return this.Dictionary.Values; }
        }
    }
}

#endif
