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

using System.Collections.Generic;

namespace NLog.Internal
{
    internal struct ScopeContextPropertyEnumerator<TValue> : IEnumerator<KeyValuePair<string, object>>
    {
        readonly IEnumerator<KeyValuePair<string, object>> _scopeEnumerator;
#if !NET35 && !NET40
        readonly IReadOnlyList<KeyValuePair<string, object>> _scopeList;
        int _scopeIndex;
#endif
        Dictionary<string, object>.Enumerator _dicationaryEnumerator;

        public ScopeContextPropertyEnumerator(IEnumerable<KeyValuePair<string, TValue>> scopeProperties)
        {
#if !NET35 && !NET40
            if (scopeProperties is IReadOnlyList<KeyValuePair<string, object>> scopeList)
            {
                _scopeEnumerator = null;
                _scopeList = scopeList;
                _scopeIndex = -1;
                _dicationaryEnumerator = default(Dictionary<string, object>.Enumerator);
                return;
            }
            else
            {
                _scopeList = null;
                _scopeIndex = 0;
            }
#endif

            if (scopeProperties is Dictionary<string, object> scopeDictionary)
            {
                _scopeEnumerator = null;
                _dicationaryEnumerator = scopeDictionary.GetEnumerator();
            }
            else if (scopeProperties is IEnumerable<KeyValuePair<string, object>> scopeEnumerator)
            {
                _scopeEnumerator = scopeEnumerator.GetEnumerator();
                _dicationaryEnumerator = default(Dictionary<string, object>.Enumerator);
            }
            else
            {
                _scopeEnumerator = CreateScopeEnumerable(scopeProperties).GetEnumerator();
                _dicationaryEnumerator = default(Dictionary<string, object>.Enumerator);
            }
        }

#if !NET35 && !NET40
        public static void CopyScopePropertiesToDictionary(IReadOnlyCollection<KeyValuePair<string, TValue>> parentContext, Dictionary<string, object> scopeDictionary)
        {
            using (var propertyEnumerator = new ScopeContextPropertyEnumerator<TValue>(parentContext))
            {
                while (propertyEnumerator.MoveNext())
                {
                    var item = propertyEnumerator.Current;
                    scopeDictionary[item.Key] = item.Value;
                }
            }
        }
#endif

        public static bool HasUniqueCollectionKeys(IEnumerable<KeyValuePair<string, TValue>> scopeProperties, IEqualityComparer<string> keyComparer)
        {
            int startIndex = 1;
            using (var leftEnumerator = new ScopeContextPropertyEnumerator<TValue>(scopeProperties))
            {
                while (leftEnumerator.MoveNext())
                {
                    ++startIndex;

                    int currentIndex = 0;

                    var left = leftEnumerator.Current;
                    using (var rightEnumerator = new ScopeContextPropertyEnumerator<TValue>(scopeProperties))
                    {
                        while (rightEnumerator.MoveNext())
                        {
                            if (++currentIndex < startIndex)
                                continue;

                            var right = rightEnumerator.Current;
                            if (keyComparer.Equals(left.Key, right.Key))
                            {
                                return false;
                            }

                            if (currentIndex > 10)
                            {
                                return false;   // Too many comparisons
                            }
                        }
                    }
                }
            }

            return true;
        }

        private static IEnumerable<KeyValuePair<string, object>> CreateScopeEnumerable(IEnumerable<KeyValuePair<string, TValue>> scopeProperties)
        {
            foreach (var property in scopeProperties)
                yield return new KeyValuePair<string, object>(property.Key, property.Value);
        }

        public KeyValuePair<string, object> Current
        {
            get
            {
#if !NET35 && !NET40
                if (_scopeList != null)
                {
                    return _scopeList[_scopeIndex];
                }
                else
#endif
                if (_scopeEnumerator != null)
                {
                    return _scopeEnumerator.Current;
                }
                else
                {
                    return _dicationaryEnumerator.Current;
                }
            }
        }

        object System.Collections.IEnumerator.Current => Current;

        public void Dispose()
        {
            if (_scopeEnumerator != null)
                _scopeEnumerator.Dispose();
            else
#if !NET35 && !NET40
            if (_scopeList is null)
#endif
                _dicationaryEnumerator.Dispose();
        }

        public bool MoveNext()
        {
#if !NET35 && !NET40
            if (_scopeList != null)
            {
                if (_scopeIndex < _scopeList.Count - 1)
                {
                    ++_scopeIndex;
                    return true;
                }
                return false;
            }
            else
#endif
            if (_scopeEnumerator != null)
            {
                return _scopeEnumerator.MoveNext();
            }
            else
            {
                return _dicationaryEnumerator.MoveNext();
            }
        }

        public void Reset()
        {
#if !NET35 && !NET40
            if (_scopeList != null)
            {
                _scopeIndex = -1;
            }
            else
#endif
            if (_scopeEnumerator != null)
            {
                _scopeEnumerator.Reset();
            }
            else
            {
                _dicationaryEnumerator = default(Dictionary<string, object>.Enumerator);
            }
        }
    }
}
