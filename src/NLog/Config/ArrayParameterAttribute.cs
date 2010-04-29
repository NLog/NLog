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

namespace NLog.Config
{
    /// <summary>
    /// Used to mark configurable parameters which are arrays. 
    /// Specifies the mapping between XML elements and .NET types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ArrayParameterAttribute: Attribute
    {
        private Type _itemType;
        private string _elementName;

        /// <summary>
        /// Creates a new instance of ArrayParameterAttribute specifying the
        /// element type and configuration element name.
        /// </summary>
        /// <param name="itemType">The type of the array item</param>
        /// <param name="elementName">The XML element name that represents the item.</param>
        public ArrayParameterAttribute(Type itemType, string elementName)
        {
            _itemType = itemType;
            _elementName = elementName;
        }

        /// <summary>
        /// The .NET type of the array item
        /// </summary>
        public Type ItemType
        {
            get { return _itemType; }
        }

        /// <summary>
        /// The XML element name.
        /// </summary>
        public string ElementName
        {
            get { return _elementName; }
        }
    }
}
