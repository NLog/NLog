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

using System;
using System.Collections;
using System.Collections.Generic;

namespace NLog.Internal
{
    /// <summary>
    /// Ensures that IDictionary.GetEnumerator returns DictionaryEntry values
    /// </summary>
    internal struct DictionaryEntryEnumerable : IEnumerable<DictionaryEntry>
    {
        private readonly IDictionary _dictionary;

        public DictionaryEntryEnumerable(IDictionary dictionary)
        {
            _dictionary = dictionary;
        }

        public DictionaryEntryEnumerator GetEnumerator()
        {
            return new DictionaryEntryEnumerator(_dictionary?.Count > 0 ? _dictionary : null);
        }

        IEnumerator<DictionaryEntry> IEnumerable<DictionaryEntry>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal struct DictionaryEntryEnumerator : IEnumerator<DictionaryEntry>
        {
            private readonly IDictionaryEnumerator _entryEnumerator;

            public DictionaryEntry Current => _entryEnumerator.Entry;

            public DictionaryEntryEnumerator(IDictionary dictionary)
            {
                _entryEnumerator = dictionary?.GetEnumerator();
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                if (_entryEnumerator is IDisposable disposable)
                    disposable.Dispose();
            }

            public bool MoveNext()
            {
                return _entryEnumerator?.MoveNext() ?? false;
            }

            public void Reset()
            {
                _entryEnumerator?.Reset();
            }
        }
    }
}
