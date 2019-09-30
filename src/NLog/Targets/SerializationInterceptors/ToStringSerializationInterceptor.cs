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

using System;
using System.Reflection;

namespace NLog.Targets.SerializationInterceptors
{
    /// <summary>
    /// An interceptor that can replace an object with it's 'ToString' method.
    /// </summary>
    public class ToStringSerializationInterceptor : ISerializationInterceptor
    {
        private readonly ISerializationInterceptor _parent;
        private readonly Type _type;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">The previous interceptor, used for chaining.</param>
        /// <param name="type"></param>
        public ToStringSerializationInterceptor(ISerializationInterceptor parent, Type type)
        {
            _parent = parent;
            _type = type;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="value"></param>
        /// <param name="replacementValue"></param>
        /// <returns></returns>
        public virtual bool GetCustomSerializedValue(Type objectType, object value, out object replacementValue)
        {
            if (_type.IsAssignableFrom(objectType))
            {
                replacementValue = value?.ToString() ?? "null";
                return true;
            }

            if (_parent != null)
                return _parent.GetCustomSerializedValue(objectType, value, out replacementValue);

            replacementValue = null;
            return false;
        }
    }

}