// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    /// <summary>
    /// Helper for extracting propertyPath
    /// </summary>
    internal class ObjectPropertyHelper
    {
        private string[] _objectPropertyPath;
        private ObjectReflectionCache _objectReflectionCache;
        private ObjectReflectionCache ObjectReflectionCache => _objectReflectionCache ?? (_objectReflectionCache = new ObjectReflectionCache());

        /// <summary>
        /// Object Path to check
        /// </summary>
        public string ObjectPath
        {
            get => _objectPropertyPath?.Length > 0 ? string.Join(".", _objectPropertyPath) : null;
            set => _objectPropertyPath = StringHelpers.IsNullOrWhiteSpace(value) ? null : value.SplitAndTrimTokens('.');
        }

        /// <summary>
        /// Try get value from <paramref name="value"/>, using <see cref="ObjectPath"/>, and set into <paramref name="foundValue"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="foundValue"></param>
        /// <returns></returns>
        public bool TryGetObjectProperty(object value, out object foundValue)
        {
            foundValue = null;

            if (_objectPropertyPath == null)
            {
                return false;
            }

            var objectReflectionCache = ObjectReflectionCache;
            for (int i = 0; i < _objectPropertyPath.Length; ++i)
            {
                if (value == null)
                {
                    // Found null
                    foundValue = null;
                    return true;
                }

                var eventProperties = objectReflectionCache.LookupObjectProperties(value);
                if (eventProperties.TryGetPropertyValue(_objectPropertyPath[i], out var propertyValue))
                {
                    value = propertyValue.Value;
                }
                else
                {
                    foundValue = null;
                    return false; //Wrong, but done
                }
            }

            foundValue = value;
            return true;
        }
    }
}