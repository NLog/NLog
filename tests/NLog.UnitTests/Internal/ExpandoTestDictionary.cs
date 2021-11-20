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

namespace NLog.UnitTests.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Special Expando-Object that has custom object-value (Similar to JObject)
    /// </summary>
    internal class ExpandoTestDictionary : IDictionary<string, IFormattable>
    {
        private readonly Dictionary<string, IFormattable> _properties = new Dictionary<string, IFormattable>();

        public IFormattable this[string key] { get => ((IDictionary<string, IFormattable>)_properties)[key]; set => ((IDictionary<string, IFormattable>)_properties)[key] = value; }

        public ICollection<string> Keys => ((IDictionary<string, IFormattable>)_properties).Keys;

        public ICollection<IFormattable> Values => ((IDictionary<string, IFormattable>)_properties).Values;

        public int Count => ((IDictionary<string, IFormattable>)_properties).Count;

        public bool IsReadOnly => ((IDictionary<string, IFormattable>)_properties).IsReadOnly;

        public void Add(string key, IFormattable value)
        {
            ((IDictionary<string, IFormattable>)_properties).Add(key, value);
        }

        public void Add(KeyValuePair<string, IFormattable> item)
        {
            ((IDictionary<string, IFormattable>)_properties).Add(item);
        }

        public void Clear()
        {
            ((IDictionary<string, IFormattable>)_properties).Clear();
        }

        public bool Contains(KeyValuePair<string, IFormattable> item)
        {
            return ((IDictionary<string, IFormattable>)_properties).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, IFormattable>)_properties).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, IFormattable>[] array, int arrayIndex)
        {
            ((IDictionary<string, IFormattable>)_properties).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, IFormattable>> GetEnumerator()
        {
            return ((IDictionary<string, IFormattable>)_properties).GetEnumerator();
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, IFormattable>)_properties).Remove(key);
        }

        public bool Remove(KeyValuePair<string, IFormattable> item)
        {
            return ((IDictionary<string, IFormattable>)_properties).Remove(item);
        }

        public bool TryGetValue(string key, out IFormattable value)
        {
            return ((IDictionary<string, IFormattable>)_properties).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, IFormattable>)_properties).GetEnumerator();
        }
    }
}
