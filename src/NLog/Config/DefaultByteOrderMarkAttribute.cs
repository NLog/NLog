// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    /// Marks a property whose type is <see cref="System.Text.Encoding"/> with a default BOM state, if one
    /// is not specified by the Encoding name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DefaultByteOrderMarkAttribute : Attribute
    {
        /// <summary>
        /// The DefaultByteOrderMarkAttribute to use if none is defined.
        /// </summary>
        public static readonly DefaultByteOrderMarkAttribute Default = new DefaultByteOrderMarkAttribute(ByteOrderMark.Unspecified);

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultByteOrderMarkAttribute"/>.
        /// </summary>
        public DefaultByteOrderMarkAttribute()
            : this(ByteOrderMark.Unspecified)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultByteOrderMarkAttribute"/>.
        /// </summary>
        /// <param name="bomState">The default state of the Byte Order Mark.</param>
        public DefaultByteOrderMarkAttribute(ByteOrderMark bomState)
        {
            UTF8 = bomState;
            UTF16 = bomState;
            UTF32 = bomState;
        }

        internal bool? GetCodepageState(int codePage)
        {
            switch (codePage)
            {
                case 1200:
                case 1201:
                    return GetByteOrderMarkState(UTF16);

                case 12000:
                case 12001:
                    return GetByteOrderMarkState(UTF32);

                case 65001:
                    return GetByteOrderMarkState(UTF8);

                default:
                    return null;
            }
        }

        private static bool? GetByteOrderMarkState ( ByteOrderMark orderMark )
        {
            return (orderMark == ByteOrderMark.Unspecified) ? (bool?)null : (orderMark == ByteOrderMark.Include);
        }

        /// <summary>
        /// Gets a value that indicates whether a BOM is included in UTF-8 encoding by default.
        /// </summary>
        public ByteOrderMark UTF8 { get; set; }

        /// <summary>
        /// Gets a value that indicates whether a BOM is included in UTF-16 encodings by default.
        /// </summary>
        public ByteOrderMark UTF16 { get; set; }

        /// <summary>
        /// Gets a value that indicates whether a BOM is included in UTF-32 encodings by default.
        /// </summary>
        public ByteOrderMark UTF32 { get; set; }
    }
}
